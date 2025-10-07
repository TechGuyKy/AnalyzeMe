using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using AnalyzeMe.Models;
using Microsoft.Win32;

namespace AnalyzeMe.Services
{
    public class ProgramManager
    {
        private List<WindowsService>? _servicesCache;
        private DateTime _servicesCacheTime = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(2);
        private readonly object _cacheLock = new object();

        #region Installed Programs

        public async Task<List<ProgramInfo>> GetInstalledProgramsAsync()
        {
            return await Task.Run(() =>
            {
                var programs = new HashSet<string>(); //this was terrible to implement overall, but this tracks duplicates
                var programList = new List<ProgramInfo>();

                var registryPaths = new List<string>
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
                };

                if (Environment.Is64BitOperatingSystem)
                {
                    registryPaths.Add(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                }

                foreach (var path in registryPaths)
                {
                    var pathPrograms = GetProgramsFromRegistry(path);

                    foreach (var program in pathPrograms)
                    {
                        var key = $"{program.Name}_{program.Publisher}_{program.Version}";
                        if (programs.Add(key))
                        {
                            programList.Add(program);
                        }
                    }
                }

                return programList.OrderBy(p => p.Name).ToList();
            });
        }

        private List<ProgramInfo> GetProgramsFromRegistry(string registryPath)
        {
            var programs = new List<ProgramInfo>();

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(registryPath);
                if (key == null) return programs;

                foreach (var subkeyName in key.GetSubKeyNames())
                {
                    try
                    {
                        using var subkey = key.OpenSubKey(subkeyName);
                        if (subkey == null) continue;

                        var displayName = subkey.GetValue("DisplayName") as string;
                        if (string.IsNullOrWhiteSpace(displayName)) continue;

                        //small code to filter out system components and updates
                        if (IsSystemComponent(subkey) || IsWindowsUpdate(subkey))
                            continue;

                        var program = CreateProgramInfo(subkey, displayName);
                        programs.Add(program);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error reading subkey {subkeyName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing registry path {registryPath}: {ex.Message}");
            }

            return programs;
        }

        private bool IsSystemComponent(RegistryKey subkey)
        {
            var systemComponent = subkey.GetValue("SystemComponent");
            return systemComponent != null && systemComponent.ToString() == "1";
        }

        private bool IsWindowsUpdate(RegistryKey subkey)
        {
            var parentKeyName = subkey.GetValue("ParentKeyName");
            return parentKeyName != null;
        }

        private ProgramInfo CreateProgramInfo(RegistryKey subkey, string displayName)
        {
            var program = new ProgramInfo
            {
                Name = displayName,
                Version = subkey.GetValue("DisplayVersion") as string,
                Publisher = subkey.GetValue("Publisher") as string,
                InstallDate = subkey.GetValue("InstallDate") as string,
                InstallLocation = subkey.GetValue("InstallLocation") as string,
                UninstallString = subkey.GetValue("UninstallString") as string
            };

            var sizeValue = subkey.GetValue("EstimatedSize");
            if (sizeValue != null && int.TryParse(sizeValue.ToString(), out int sizeKB))
            {
                program.SizeMB = sizeKB / 1024.0;
            }

            return program;
        }

        public async Task<bool> UninstallProgramAsync(ProgramInfo program)
        {
            if (string.IsNullOrEmpty(program.UninstallString))
                return false;

            return await Task.Run(() =>
            {
                try
                {
                    var (fileName, arguments) = ParseUninstallString(program.UninstallString);

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    var process = Process.Start(startInfo);
                    return process != null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error uninstalling {program.Name}: {ex.Message}");
                    return false;
                }
            });
        }

        private (string fileName, string arguments) ParseUninstallString(string uninstallString)
        {
            uninstallString = uninstallString.Trim();

            if (uninstallString.StartsWith("\""))
            {
                var endQuote = uninstallString.IndexOf("\"", 1);
                if (endQuote > 0)
                {
                    var fileName = uninstallString.Substring(1, endQuote - 1);
                    var arguments = uninstallString.Length > endQuote + 1
                        ? uninstallString.Substring(endQuote + 1).Trim()
                        : "";
                    return (fileName, arguments);
                }
            }

            var spaceIndex = uninstallString.IndexOf(' ');
            if (spaceIndex > 0)
            {
                return (uninstallString.Substring(0, spaceIndex),
                        uninstallString.Substring(spaceIndex + 1).Trim());
            }

            return (uninstallString, "");
        }

        #endregion

        #region Startup Programs

        public async Task<List<StartupProgram>> GetStartupProgramsAsync()
        {
            return await Task.Run(() =>
            {
                var startupPrograms = new List<StartupProgram>();

                var registryLocations = new[]
                {
                    (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKCU"),
                    (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKLM")
                };

                foreach (var (baseKey, path, hive) in registryLocations)
                {
                    startupPrograms.AddRange(GetStartupFromRegistry(baseKey, path, hive));
                }

                return startupPrograms.OrderBy(s => s.Name).ToList();
            });
        }

        private List<StartupProgram> GetStartupFromRegistry(RegistryKey baseKey, string path, string hive)
        {
            var programs = new List<StartupProgram>();

            try
            {
                using var key = baseKey.OpenSubKey(path);
                if (key == null) return programs;

                foreach (var valueName in key.GetValueNames())
                {
                    try
                    {
                        var command = key.GetValue(valueName) as string;
                        if (string.IsNullOrEmpty(command)) continue;

                        programs.Add(new StartupProgram
                        {
                            Name = valueName,
                            Command = command,
                            Location = $"{hive}\\{path}",
                            IsEnabled = true
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error reading startup value {valueName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing startup registry {hive}\\{path}: {ex.Message}");
            }

            return programs;
        }

        public async Task<bool> DisableStartupProgramAsync(StartupProgram program)
        {
            if (string.IsNullOrEmpty(program.Location) || string.IsNullOrEmpty(program.Name))
                return false;

            return await Task.Run(() =>
            {
                try
                {
                    var isHKCU = program.Location.StartsWith("HKCU");
                    var path = program.Location.Replace("HKCU\\", "").Replace("HKLM\\", "");

                    var baseKey = isHKCU ? Registry.CurrentUser : Registry.LocalMachine;
                    using var key = baseKey.OpenSubKey(path, writable: true);

                    if (key == null)
                    {
                        Debug.WriteLine($"Could not open registry key: {path}");
                        return false;
                    }

                    key.DeleteValue(program.Name, throwOnMissingValue: false);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disabling startup program {program.Name}: {ex.Message}");
                    return false;
                }
            });
        }

        #endregion

        #region Windows Services

        public async Task<List<WindowsService>> GetWindowsServicesAsync()
        {
            lock (_cacheLock)
            {
                if (_servicesCache != null &&
                    DateTime.Now - _servicesCacheTime < _cacheExpiration)
                {
                    Debug.WriteLine($"Returning cached services ({_servicesCache.Count} items)");
                    return _servicesCache;
                }
            }

            return await Task.Run(() =>
            {
                var services = new List<WindowsService>();

                try
                {
                    Debug.WriteLine("Querying Windows services...");

                    var serviceControllers = ServiceController.GetServices();
                    Debug.WriteLine($"Found {serviceControllers.Length} services");

                    foreach (var sc in serviceControllers)
                    {
                        try
                        {
                            services.Add(new WindowsService
                            {
                                Name = sc.ServiceName,
                                DisplayName = sc.DisplayName,
                                Status = sc.Status.ToString(),
                                StartupType = sc.StartType.ToString(),
                                Description = GetServiceDescription(sc.ServiceName)
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error reading service {sc.ServiceName}: {ex.Message}");
                        }
                        finally
                        {
                            sc.Dispose();
                        }
                    }

                    lock (_cacheLock)
                    {
                        _servicesCache = services.OrderBy(s => s.DisplayName).ToList();
                        _servicesCacheTime = DateTime.Now;
                        Debug.WriteLine($"Cached {_servicesCache.Count} services");
                    }

                    return _servicesCache;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting services: {ex.Message}");

                    lock (_cacheLock)
                    {
                        return _servicesCache ?? new List<WindowsService>();
                    }
                }
            });
        }

        private string GetServiceDescription(string serviceName)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
                return key?.GetValue("Description") as string ?? "";
            }
            catch
            {
                return "";
            }
        }

        public async Task<bool> ChangeServiceStartupTypeAsync(string serviceName, string startupType)
        {
            if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(startupType))
                return false;

            return await Task.Run(() =>
            {
                try
                {
                    var serviceStartMode = ParseStartupType(startupType);

                    using var service = new ServiceController(serviceName);

                    //this uses sc.exe command for changing startup type which is the most reliable method i've found
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"config \"{serviceName}\" start= {serviceStartMode}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Verb = "runas"
                    };

                    using var process = Process.Start(startInfo);
                    if (process == null) return false;

                    process.WaitForExit(5000);
                    var success = process.ExitCode == 0;

                    if (success)
                    {
                        ClearCache();
                        Debug.WriteLine($"Changed {serviceName} startup type to {startupType}");
                    }
                    else
                    {
                        var error = process.StandardError.ReadToEnd();
                        Debug.WriteLine($"Failed to change service startup: {error}");
                    }

                    return success;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error changing service startup type: {ex.Message}");
                    return false;
                }
            });
        }

        private string ParseStartupType(string startupType)
        {
            return startupType.ToLower() switch
            {
                "automatic" => "auto",
                "manual" => "demand",
                "disabled" => "disabled",
                _ => "demand"
            };
        }

        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _servicesCache = null;
                _servicesCacheTime = DateTime.MinValue;
                Debug.WriteLine("Service cache cleared");
            }
        }

        public async Task RefreshServicesAsync()
        {
            ClearCache();
            await GetWindowsServicesAsync();
        }

        #endregion
    }
}