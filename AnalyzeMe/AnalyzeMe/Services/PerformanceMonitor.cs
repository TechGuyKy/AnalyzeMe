using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using AnalyzeMe.Models;

namespace AnalyzeMe.Services
{
    public class PerformanceMonitor
    {
        private readonly PerformanceCounter _cpuCounter;
        private PerformanceCounter? _ramCounter;

        public PerformanceMonitor()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            try
            {
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                _ramCounter.NextValue();
            }
            catch
            {
                _ramCounter = null;
            }

            _cpuCounter.NextValue();
        }

        public async Task<PerformanceMetrics> GetCurrentMetricsAsync()
        {
            return await Task.Run(() =>
            {
                var metrics = new PerformanceMetrics
                {
                    CPUUsage = Math.Round(_cpuCounter.NextValue(), 2),
                    RAMUsage = GetRAMUsagePercentage(),
                    DiskUsage = GetDiskUsagePercentage(),
                    NetworkUsage = 0,
                    ProcessCount = Process.GetProcesses().Length,
                    ThreadCount = GetTotalThreadCount(),
                    HandleCount = GetTotalHandleCount(),
                    PageFileUsage = GetPageFileUsage(),
                    CPUTemperature = 0,
                    TopProcesses = GetTopProcesses(),
                    Timestamp = DateTime.Now
                };

                return metrics;
            });
        }

        private double GetRAMUsagePercentage()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                long totalCapacity = 0;
                foreach (ManagementObject obj in searcher.Get())
                {
                    totalCapacity += Convert.ToInt64(obj["Capacity"]);
                }

                double availableMB = _ramCounter?.NextValue() ?? 0;
                double totalMB = totalCapacity / (1024.0 * 1024);

                if (totalMB > 0)
                {
                    double usedMB = totalMB - availableMB;
                    return Math.Round((usedMB / totalMB) * 100, 2);
                }
            }
            catch { }
            return 0;
        }

        private double GetDiskUsagePercentage()
        {
            try
            {
                var drives = System.IO.DriveInfo.GetDrives()
                    .Where(d => d.DriveType == System.IO.DriveType.Fixed && d.IsReady)
                    .ToList();

                if (drives.Any())
                {
                    var totalSpace = drives.Sum(d => d.TotalSize);
                    var usedSpace = drives.Sum(d => d.TotalSize - d.AvailableFreeSpace);
                    return Math.Round((double)usedSpace / totalSpace * 100, 2);
                }
            }
            catch { }
            return 0;
        }

        private double GetPageFileUsage()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileUsage");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var allocatedSize = Convert.ToDouble(obj["AllocatedBaseSize"]);
                    var currentUsage = Convert.ToDouble(obj["CurrentUsage"]);
                    if (allocatedSize > 0)
                        return Math.Round((currentUsage / allocatedSize) * 100, 2);
                }
            }
            catch { }
            return 0;
        }

        private int GetTotalThreadCount()
        {
            try
            {
                return Process.GetProcesses().Sum(p =>
                {
                    try { return p.Threads.Count; }
                    catch { return 0; }
                });
            }
            catch { }
            return 0;
        }

        private int GetTotalHandleCount()
        {
            try
            {
                return Process.GetProcesses().Sum(p =>
                {
                    try { return p.HandleCount; }
                    catch { return 0; }
                });
            }
            catch { }
            return 0;
        }

        private System.Collections.Generic.List<ProcessInfo> GetTopProcesses()
        {
            try
            {
                return Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                    .OrderByDescending(p =>
                    {
                        try { return p.WorkingSet64; }
                        catch { return 0; }
                    })
                    .Take(10)
                    .Select(p => new ProcessInfo
                    {
                        Name = p.ProcessName,
                        Id = p.Id,
                        CPUUsage = 0,
                        MemoryUsage = p.WorkingSet64 / (1024.0 * 1024),
                        Status = p.Responding ? "Running" : "Not Responding",
                        ThreadCount = GetProcessThreadCount(p),
                        HandleCount = GetProcessHandleCount(p)
                    })
                    .ToList();
            }
            catch
            {
                return new System.Collections.Generic.List<ProcessInfo>();
            }
        }

        private int GetProcessThreadCount(Process p)
        {
            try { return p.Threads.Count; }
            catch { return 0; }
        }

        private int GetProcessHandleCount(Process p)
        {
            try { return p.HandleCount; }
            catch { return 0; }
        }
    }
}