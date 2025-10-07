using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeMe.Models;

namespace AnalyzeMe.Services
{
    public class DiagnosticService
    {
        public async Task<List<DiagnosticResult>> RunAllDiagnosticsAsync(
            SystemInfo systemInfo,
            PerformanceMetrics metrics)
        {
            return await Task.Run(() =>
            {
                var results = new List<DiagnosticResult>();

                results.AddRange(CheckCPUHealth(metrics));
                results.AddRange(CheckMemoryHealth(systemInfo, metrics));
                results.AddRange(CheckDiskHealth(systemInfo));
                results.AddRange(CheckSystemUptime(systemInfo));
                results.AddRange(CheckProcessCount(metrics));
                results.AddRange(CheckHardwareConfiguration(systemInfo));
                results.AddRange(CheckStorageHealth(systemInfo));

                return results.OrderByDescending(r => r.Severity).ToList();
            });
        }

        private List<DiagnosticResult> CheckCPUHealth(PerformanceMetrics metrics)
        {
            var results = new List<DiagnosticResult>();

            if (metrics.CPUUsage > 95)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "CPU",
                    Title = "Critical CPU Usage",
                    Description = $"CPU usage is critically high at {metrics.CPUUsage:F1}%. System may become unresponsive.",
                    Severity = DiagnosticSeverity.Critical,
                    Resolution = "Close resource-intensive applications immediately or restart high-usage processes."
                });
            }
            else if (metrics.CPUUsage > 85)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "CPU",
                    Title = "High CPU Usage",
                    Description = $"CPU usage is at {metrics.CPUUsage:F1}%, which may impact system performance.",
                    Severity = DiagnosticSeverity.Warning,
                    Resolution = "Check Task Manager for high CPU processes and close unnecessary applications."
                });
            }
            else if (metrics.CPUUsage < 5)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "CPU",
                    Title = "Optimal CPU Performance",
                    Description = $"CPU usage is healthy at {metrics.CPUUsage:F1}%.",
                    Severity = DiagnosticSeverity.Info,
                    Resolution = "No action needed. System is performing optimally."
                });
            }

            return results;
        }

        private List<DiagnosticResult> CheckMemoryHealth(SystemInfo systemInfo, PerformanceMetrics metrics)
        {
            var results = new List<DiagnosticResult>();

            if (systemInfo.RAMUsagePercentage > 95)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Memory",
                    Title = "Critical Memory Usage",
                    Description = $"RAM usage is critically high at {systemInfo.RAMUsagePercentage:F1}%. System stability at risk.",
                    Severity = DiagnosticSeverity.Critical,
                    Resolution = "Close applications immediately or restart the system. Consider upgrading RAM."
                });
            }
            else if (systemInfo.RAMUsagePercentage > 85)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Memory",
                    Title = "High Memory Usage",
                    Description = $"RAM usage is at {systemInfo.RAMUsagePercentage:F1}% ({systemInfo.TotalRAM - systemInfo.AvailableRAM:F1} GB / {systemInfo.TotalRAM:F1} GB used).",
                    Severity = DiagnosticSeverity.Warning,
                    Resolution = "Close unused applications, clear browser tabs, or restart memory-intensive programs."
                });
            }

            if (systemInfo.TotalRAM < 8)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Memory",
                    Title = "Insufficient System RAM",
                    Description = $"Your system has {systemInfo.TotalRAM:F1} GB of RAM, which is below modern requirements.",
                    Severity = DiagnosticSeverity.Warning,
                    Resolution = "Upgrade to at least 16 GB RAM for optimal performance with modern applications."
                });
            }
            else if (systemInfo.TotalRAM >= 32)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Memory",
                    Title = "Excellent RAM Configuration",
                    Description = $"Your system has {systemInfo.TotalRAM:F1} GB of RAM, which is excellent for multitasking.",
                    Severity = DiagnosticSeverity.Info,
                    Resolution = "No action needed. RAM capacity is more than sufficient."
                });
            }

            return results;
        }

        private List<DiagnosticResult> CheckDiskHealth(SystemInfo systemInfo)
        {
            var results = new List<DiagnosticResult>();

            if (systemInfo.DiskUsagePercentage > 95)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Storage",
                    Title = "Critical Disk Space",
                    Description = $"Disk usage is critically high at {systemInfo.DiskUsagePercentage:F1}% with only {systemInfo.FreeDiskSpace:F1} GB free.",
                    Severity = DiagnosticSeverity.Critical,
                    Resolution = "Delete unnecessary files immediately, run Disk Cleanup, or add additional storage."
                });
            }
            else if (systemInfo.DiskUsagePercentage > 90)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Storage",
                    Title = "Low Disk Space",
                    Description = $"Disk usage is at {systemInfo.DiskUsagePercentage:F1}% with {systemInfo.FreeDiskSpace:F1} GB free.",
                    Severity = DiagnosticSeverity.Error,
                    Resolution = "Free up disk space by removing temporary files, old downloads, or moving files to external storage."
                });
            }
            else if (systemInfo.DiskUsagePercentage > 80)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Storage",
                    Title = "Disk Space Warning",
                    Description = $"Disk usage is at {systemInfo.DiskUsagePercentage:F1}%.",
                    Severity = DiagnosticSeverity.Warning,
                    Resolution = "Consider cleaning up disk space to maintain optimal performance."
                });
            }

            return results;
        }

        private List<DiagnosticResult> CheckSystemUptime(SystemInfo systemInfo)
        {
            var results = new List<DiagnosticResult>();

            if (systemInfo.Uptime.TotalDays > 30)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "System",
                    Title = "Extended System Uptime",
                    Description = $"System has been running for {systemInfo.Uptime.Days} days without a restart.",
                    Severity = DiagnosticSeverity.Warning,
                    Resolution = "Restart your system to apply pending updates, clear memory leaks, and improve stability."
                });
            }
            else if (systemInfo.Uptime.TotalDays > 14)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "System",
                    Title = "Long System Uptime",
                    Description = $"System has been running for {systemInfo.Uptime.Days} days.",
                    Severity = DiagnosticSeverity.Info,
                    Resolution = "Consider restarting soon to maintain optimal performance."
                });
            }

            return results;
        }

        private List<DiagnosticResult> CheckProcessCount(PerformanceMetrics metrics)
        {
            var results = new List<DiagnosticResult>();

            if (metrics.ProcessCount > 400)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Processes",
                    Title = "Very High Process Count",
                    Description = $"System is running {metrics.ProcessCount} processes, which is unusually high.",
                    Severity = DiagnosticSeverity.Warning,
                    Resolution = "Review running processes in Task Manager and close unnecessary applications."
                });
            }
            else if (metrics.ProcessCount > 300)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Processes",
                    Title = "High Process Count",
                    Description = $"System is running {metrics.ProcessCount} processes.",
                    Severity = DiagnosticSeverity.Info,
                    Resolution = "Monitor for performance issues and close unused applications if needed."
                });
            }

            return results;
        }

        private List<DiagnosticResult> CheckHardwareConfiguration(SystemInfo systemInfo)
        {
            var results = new List<DiagnosticResult>();

            //this checks the total processor cores
            if (systemInfo.ProcessorCores < 4)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Hardware",
                    Title = "Low CPU Core Count",
                    Description = $"Your CPU has only {systemInfo.ProcessorCores} cores, which may limit multitasking performance.",
                    Severity = DiagnosticSeverity.Info,
                    Resolution = "Consider upgrading to a CPU with 6 or more cores for better multitasking."
                });
            }

            //this checks the total number of memory slots on the motherboard
            if (systemInfo.MemorySlotsUsed < systemInfo.MemorySlots && systemInfo.TotalRAM < 16)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Hardware",
                    Title = "Memory Expansion Available",
                    Description = $"You have {systemInfo.MemorySlotsUsed} of {systemInfo.MemorySlots} memory slots in use. Additional RAM can be added.",
                    Severity = DiagnosticSeverity.Info,
                    Resolution = $"Consider adding more RAM modules to reach at least 16 GB total."
                });
            }

            return results;
        }

        private List<DiagnosticResult> CheckStorageHealth(SystemInfo systemInfo)
        {
            var results = new List<DiagnosticResult>();

            var ssdCount = systemInfo.Disks.Count(d => d.MediaType == "SSD");
            var hddCount = systemInfo.Disks.Count(d => d.MediaType == "HDD");

            if (hddCount > 0 && ssdCount == 0)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Storage",
                    Title = "No SSD Detected",
                    Description = "Your system uses only HDDs. SSDs provide significantly faster performance.",
                    Severity = DiagnosticSeverity.Info,
                    Resolution = "Consider upgrading to an SSD for faster boot times and application loading."
                });
            }
            else if (ssdCount > 0)
            {
                results.Add(new DiagnosticResult
                {
                    Category = "Storage",
                    Title = "SSD Storage Detected",
                    Description = $"Your system has {ssdCount} SSD(s), providing excellent storage performance.",
                    Severity = DiagnosticSeverity.Info,
                    Resolution = "No action needed. SSD configuration is optimal."
                });
            }

            return results;
        }
    }
}