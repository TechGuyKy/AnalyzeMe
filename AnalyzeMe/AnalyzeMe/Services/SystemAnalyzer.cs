using System;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using AnalyzeMe.Models;
using System.Collections.Generic;

namespace AnalyzeMe.Services
{
    public class SystemAnalyzer
    {
        public async Task<SystemInfo> GetSystemInfoAsync()
        {
            return await Task.Run(() =>
            {
                var systemInfo = new SystemInfo
                {
                    ComputerName = Environment.MachineName,
                    OperatingSystem = GetOperatingSystemInfo(),
                    WindowsVersion = GetWindowsVersion(),
                    SystemArchitecture = GetSystemArchitecture(),
                    ProcessorName = GetProcessorInfo(),
                    ProcessorCores = GetPhysicalCores(),
                    ProcessorThreads = Environment.ProcessorCount,
                    ProcessorBaseSpeed = GetProcessorSpeed(),
                    ProcessorMaxSpeed = GetProcessorMaxSpeed(),
                    TotalRAM = GetTotalRAM(),
                    AvailableRAM = GetAvailableRAM(),
                    MemorySpeed = GetMemorySpeed(),
                    MemoryType = GetMemoryType(),
                    MemorySlots = GetMemorySlots(),
                    MemorySlotsUsed = GetMemorySlotsUsed(),
                    GraphicsCard = GetGraphicsCardInfo(),
                    GraphicsMemory = GetGraphicsMemory(),
                    MotherboardName = GetMotherboardInfo(),
                    BiosVersion = GetBiosVersion(),
                    BiosDate = GetBiosDate(),
                    Disks = GetDiskInfo(),
                    TotalDiskSpace = GetTotalDiskSpace(),
                    FreeDiskSpace = GetFreeDiskSpace(),
                    LastBootTime = GetLastBootTime()
                };

                return systemInfo;
            });
        }

        private string GetOperatingSystemInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var caption = obj["Caption"]?.ToString() ?? "Unknown";
                    var buildNumber = obj["BuildNumber"]?.ToString() ?? "";
                    return $"{caption} (Build {buildNumber})";
                }
            }
            catch { }
            return "Unknown OS";
        }

        private string GetWindowsVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["Version"]?.ToString() ?? "Unknown";
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetSystemArchitecture()
        {
            return Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        }

        private string GetProcessorInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["Name"]?.ToString()?.Trim() ?? "Unknown CPU";
                }
            }
            catch { }
            return "Unknown CPU";
        }

        private int GetPhysicalCores()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["NumberOfCores"]);
                }
            }
            catch { }
            return Environment.ProcessorCount / 2;
        }

        private double GetProcessorSpeed()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToDouble(obj["CurrentClockSpeed"]) / 1000.0;
                }
            }
            catch { }
            return 0;
        }

        private double GetProcessorMaxSpeed()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToDouble(obj["MaxClockSpeed"]) / 1000.0;
                }
            }
            catch { }
            return 0;
        }

        private double GetTotalRAM()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                long totalCapacity = 0;
                foreach (ManagementObject obj in searcher.Get())
                {
                    totalCapacity += Convert.ToInt64(obj["Capacity"]);
                }
                return totalCapacity / (1024.0 * 1024 * 1024);
            }
            catch { }
            return 0;
        }

        private double GetAvailableRAM()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var freeMemory = Convert.ToDouble(obj["FreePhysicalMemory"]);
                    return (freeMemory * 1024) / (1024.0 * 1024 * 1024);
                }
            }
            catch { }
            return 0;
        }

        private string GetMemorySpeed()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var speed = obj["Speed"]?.ToString();
                    if (!string.IsNullOrEmpty(speed))
                        return $"{speed} MHz";
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetMemoryType()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var memType = Convert.ToInt32(obj["SMBIOSMemoryType"]);
                    return memType switch
                    {
                        26 => "DDR4",
                        34 => "DDR5",
                        24 => "DDR3",
                        22 => "DDR2",
                        21 => "DDR",
                        _ => $"Type {memType}"
                    };
                }
            }
            catch { }
            return "Unknown";
        }

        private int GetMemorySlots()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemoryArray");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["MemoryDevices"]);
                }
            }
            catch { }
            return 0;
        }

        private int GetMemorySlotsUsed()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                int count = 0;
                foreach (ManagementObject obj in searcher.Get())
                {
                    count++;
                }
                return count;
            }
            catch { }
            return 0;
        }
        // TODO: Need to exclude anything that's not dedicated or integrated, such as Oculus and whatnot.
        private string GetGraphicsCardInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                var gpus = new List<string>();
                foreach (ManagementObject obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString();
                    if (!string.IsNullOrEmpty(name) && !name.Contains("Microsoft Basic"))
                    {
                        gpus.Add(name);
                    }
                }
                return gpus.Any() ? string.Join(", ", gpus) : "Unknown GPU";
            }
            catch { }
            return "Unknown GPU";
        }
        // TODO: Need to fix the conversion for the graphics memory. Users are seeing different results, inaccurate readings.
        private double GetGraphicsMemory()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var adapterRam = obj["AdapterRAM"];
                    if (adapterRam != null)
                    {
                        var ram = Convert.ToDouble(adapterRam);
                        if (ram > 0)
                            return ram / (1024.0 * 1024 * 1024);
                    }
                }
            }
            catch { }
            return 0;
        }

        private string GetMotherboardInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    var product = obj["Product"]?.ToString() ?? "";
                    return $"{manufacturer} {product}".Trim();
                }
            }
            catch { }
            return "Unknown Motherboard";
        }

        private string GetBiosVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    var version = obj["SMBIOSBIOSVersion"]?.ToString() ?? "";
                    return $"{manufacturer} {version}".Trim();
                }
            }
            catch { }
            return "Unknown BIOS";
        }

        private string GetBiosDate()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var releaseDate = obj["ReleaseDate"]?.ToString();
                    if (!string.IsNullOrEmpty(releaseDate))
                    {
                        var date = ManagementDateTimeConverter.ToDateTime(releaseDate);
                        return date.ToString("yyyy-MM-dd");
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private List<DiskInfo> GetDiskInfo()
        {
            var disks = new List<DiskInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var size = Convert.ToDouble(obj["Size"] ?? 0) / (1024.0 * 1024 * 1024);
                    var mediaType = obj["MediaType"]?.ToString() ?? "";
                    var model = obj["Model"]?.ToString() ?? "Unknown";

                    var disk = new DiskInfo
                    {
                        Model = model,
                        Size = size,
                        InterfaceType = obj["InterfaceType"]?.ToString() ?? "Unknown",
                        SerialNumber = obj["SerialNumber"]?.ToString()?.Trim() ?? "N/A",
                        MediaType = DetermineMediaType(mediaType, model),
                        Partitions = Convert.ToInt32(obj["Partitions"] ?? 0)
                    };
                    disks.Add(disk);
                }
            }
            catch { }
            return disks;
        }

        private string DetermineMediaType(string mediaType, string model)
        {
            var modelLower = model.ToLower();
            if (modelLower.Contains("ssd") || modelLower.Contains("nvme") ||
                modelLower.Contains("solid state"))
                return "SSD";
            if (mediaType.Contains("SSD"))
                return "SSD";
            return "HDD";
        }

        private double GetTotalDiskSpace()
        {
            try
            {
                return System.IO.DriveInfo.GetDrives()
                    .Where(d => d.DriveType == System.IO.DriveType.Fixed && d.IsReady)
                    .Sum(d => d.TotalSize / (1024.0 * 1024 * 1024));
            }
            catch { }
            return 0;
        }

        private double GetFreeDiskSpace()
        {
            try
            {
                return System.IO.DriveInfo.GetDrives()
                    .Where(d => d.DriveType == System.IO.DriveType.Fixed && d.IsReady)
                    .Sum(d => d.AvailableFreeSpace / (1024.0 * 1024 * 1024));
            }
            catch { }
            return 0;
        }

        private DateTime GetLastBootTime()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var lastBootUp = obj["LastBootUpTime"]?.ToString();
                    if (!string.IsNullOrEmpty(lastBootUp))
                    {
                        return ManagementDateTimeConverter.ToDateTime(lastBootUp);
                    }
                }
            }
            catch { }
            return DateTime.Now.AddHours(-1);
        }
    }
}