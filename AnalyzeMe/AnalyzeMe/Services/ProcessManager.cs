using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AnalyzeMe.Models;

namespace AnalyzeMe.Services
{
    public class ProcessManager
    {
        private Dictionary<int, DateTime> _lastCpuCheck = new();
        private Dictionary<int, TimeSpan> _lastCpuTime = new();

        public async Task<List<TaskProcessInfo>> GetRunningProcessesAsync()
        {
            return await Task.Run(() =>
            {
                var processList = new List<TaskProcessInfo>();

                try
                {
                    var processes = Process.GetProcesses();

                    foreach (var process in processes)
                    {
                        try
                        {
                            var processInfo = new TaskProcessInfo
                            {
                                ProcessId = process.Id,
                                Name = process.ProcessName,
                                MemoryBytes = process.WorkingSet64,
                                MemoryMB = process.WorkingSet64 / (1024 * 1024),
                                ThreadCount = process.Threads.Count,
                                HandleCount = process.HandleCount,
                                Responding = process.Responding,
                                Status = process.Responding ? "Running" : "Not Responding"
                            };

                            // Try to get additional info
                            try
                            {
                                processInfo.StartTime = process.StartTime;
                                processInfo.PriorityClass = process.PriorityClass.ToString();
                                processInfo.FilePath = process.MainModule?.FileName;
                                processInfo.Description = process.MainModule?.FileVersionInfo.FileDescription;
                                processInfo.Publisher = process.MainModule?.FileVersionInfo.CompanyName;
                            }
                            catch
                            {
                                processInfo.IsSystemProcess = true;
                                processInfo.Description = "System Process";
                                processInfo.StartTime = DateTime.Now;
                            }

                            // Calculate CPU usage
                            processInfo.CpuUsage = GetProcessCpuUsage(process);

                            processList.Add(processInfo);
                        }
                        catch
                        {
                            // Skip processes we can't access
                            continue;
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
                catch { }

                return processList.OrderByDescending(p => p.CpuUsage).ToList();
            });
        }

        private double GetProcessCpuUsage(Process process)
        {
            try
            {
                var processId = process.Id;
                var currentTime = DateTime.Now;

                if (!_lastCpuCheck.ContainsKey(processId))
                {
                    _lastCpuCheck[processId] = currentTime;
                    _lastCpuTime[processId] = process.TotalProcessorTime;
                    return 0;
                }

                var timeDiff = (currentTime - _lastCpuCheck[processId]).TotalMilliseconds;
                if (timeDiff < 500) // Update at most every 500ms
                    return 0;

                var currentCpuTime = process.TotalProcessorTime;
                var cpuTimeDiff = (currentCpuTime - _lastCpuTime[processId]).TotalMilliseconds;

                _lastCpuCheck[processId] = currentTime;
                _lastCpuTime[processId] = currentCpuTime;

                var cpuUsage = (cpuTimeDiff / timeDiff) * 100.0 / Environment.ProcessorCount;
                return Math.Min(cpuUsage, 100);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> KillProcessAsync(int processId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var process = Process.GetProcessById(processId);
                    process.Kill();
                    process.WaitForExit(5000);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> SetProcessPriorityAsync(int processId, string priority)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var process = Process.GetProcessById(processId);

                    var priorityClass = priority switch
                    {
                        "Idle" => ProcessPriorityClass.Idle,
                        "Below Normal" => ProcessPriorityClass.BelowNormal,
                        "Normal" => ProcessPriorityClass.Normal,
                        "Above Normal" => ProcessPriorityClass.AboveNormal,
                        "High" => ProcessPriorityClass.High,
                        "Realtime" => ProcessPriorityClass.RealTime,
                        _ => ProcessPriorityClass.Normal
                    };

                    process.PriorityClass = priorityClass;
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> SuspendProcessAsync(int processId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var process = Process.GetProcessById(processId);
                    foreach (ProcessThread thread in process.Threads)
                    {
                        var threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                        if (threadHandle != IntPtr.Zero)
                        {
                            SuspendThread(threadHandle);
                            CloseHandle(threadHandle);
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> ResumeProcessAsync(int processId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var process = Process.GetProcessById(processId);
                    foreach (ProcessThread thread in process.Threads)
                    {
                        var threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                        if (threadHandle != IntPtr.Zero)
                        {
                            ResumeThread(threadHandle);
                            CloseHandle(threadHandle);
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public void ClearCpuCache()
        {
            _lastCpuCheck.Clear();
            _lastCpuTime.Clear();
        }

        // P/Invoke declarations for suspend/resume
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        private enum ThreadAccess : int
        {
            SUSPEND_RESUME = 0x0002
        }
    }
}