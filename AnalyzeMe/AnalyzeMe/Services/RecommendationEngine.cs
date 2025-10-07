using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeMe.Models;

namespace AnalyzeMe.Services
{
    public class RecommendationEngine
    {
        public async Task<List<Recommendation>> GenerateRecommendationsAsync(
            SystemInfo systemInfo,
            PerformanceMetrics metrics,
            IEnumerable<DiagnosticResult> diagnostics)
        {
            return await Task.Run(() =>
            {
                var recommendations = new List<Recommendation>();

                recommendations.AddRange(GeneratePerformanceRecommendations(systemInfo, metrics));
                recommendations.AddRange(GenerateHardwareRecommendations(systemInfo));
                recommendations.AddRange(GenerateMaintenanceRecommendations(systemInfo, metrics));
                recommendations.AddRange(GenerateDiagnosticRecommendations(diagnostics));
                recommendations.AddRange(GenerateOptimizationRecommendations(systemInfo, metrics));

                return recommendations
                    .OrderByDescending(r => r.Priority)
                    .ThenByDescending(r => r.EstimatedImpact)
                    .ToList();
            });
        }

        private List<Recommendation> GeneratePerformanceRecommendations(
            SystemInfo systemInfo,
            PerformanceMetrics metrics)
        {
            var recommendations = new List<Recommendation>();

            if (metrics.CPUUsage > 80)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Optimize CPU Usage",
                    Description = "Your CPU is consistently running at high utilization, which may cause slowdowns.",
                    Action = "Disable startup programs, close background applications, update drivers, and scan for malware.",
                    Priority = RecommendationPriority.High,
                    Category = "Performance",
                    EstimatedImpact = 30
                });
            }

            if (systemInfo.RAMUsagePercentage > 80)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Reduce Memory Usage",
                    Description = "High RAM usage can cause slowdowns and application crashes.",
                    Action = "Close memory-intensive applications, disable browser extensions, and consider upgrading RAM to at least 16GB.",
                    Priority = RecommendationPriority.High,
                    Category = "Performance",
                    EstimatedImpact = 35
                });
            }

            if (metrics.PageFileUsage > 75)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "High Page File Usage",
                    Description = "Excessive page file usage indicates RAM shortage, causing performance degradation.",
                    Action = "Close unnecessary applications and upgrade physical RAM to reduce reliance on virtual memory.",
                    Priority = RecommendationPriority.Medium,
                    Category = "Performance",
                    EstimatedImpact = 25
                });
            }

            return recommendations;
        }

        private List<Recommendation> GenerateHardwareRecommendations(SystemInfo systemInfo)
        {
            var recommendations = new List<Recommendation>();

            if (systemInfo.TotalRAM < 16)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Upgrade System RAM",
                    Description = $"Your system has {systemInfo.TotalRAM:F0}GB RAM. Modern applications benefit significantly from 16GB or more.",
                    Action = $"Upgrade to 16GB or 32GB RAM. You have {systemInfo.MemorySlots - systemInfo.MemorySlotsUsed} empty slot(s) available.",
                    Priority = RecommendationPriority.High,
                    Category = "Hardware",
                    EstimatedImpact = 45
                });
            }

            if (systemInfo.ProcessorCores < 6)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Consider CPU Upgrade",
                    Description = $"Your {systemInfo.ProcessorCores}-core CPU may struggle with modern multitasking demands.",
                    Action = "Consider upgrading to a CPU with 6-8 cores for better multitasking and application performance.",
                    Priority = RecommendationPriority.Low,
                    Category = "Hardware",
                    EstimatedImpact = 35
                });
            }

            if (systemInfo.DiskUsagePercentage > 75)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Expand Storage Capacity",
                    Description = $"Your disk is {systemInfo.DiskUsagePercentage:F0}% full with only {systemInfo.FreeDiskSpace:F0}GB free.",
                    Action = "Add an additional SSD/HDD or upgrade to a larger capacity drive to prevent performance issues.",
                    Priority = RecommendationPriority.Medium,
                    Category = "Hardware",
                    EstimatedImpact = 20
                });
            }

            var hasOnlyHDD = systemInfo.Disks.All(d => d.MediaType == "HDD");
            if (hasOnlyHDD)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Upgrade to SSD Storage",
                    Description = "HDDs are significantly slower than SSDs. An SSD upgrade provides the most noticeable performance improvement.",
                    Action = "Replace your primary HDD with an SSD (NVMe preferred) for 5-10x faster boot and load times.",
                    Priority = RecommendationPriority.High,
                    Category = "Hardware",
                    EstimatedImpact = 60
                });
            }

            return recommendations;
        }

        private List<Recommendation> GenerateMaintenanceRecommendations(
            SystemInfo systemInfo,
            PerformanceMetrics metrics)
        {
            var recommendations = new List<Recommendation>();

            if (systemInfo.Uptime.TotalDays > 7)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Restart Your System",
                    Description = $"Your system has been running for {systemInfo.Uptime.Days} days. Regular restarts improve performance and stability.",
                    Action = "Restart your computer to clear memory, apply updates, and refresh system resources.",
                    Priority = systemInfo.Uptime.TotalDays > 30 ? RecommendationPriority.High : RecommendationPriority.Low,
                    Category = "Maintenance",
                    EstimatedImpact = 15
                });
            }

            recommendations.Add(new Recommendation
            {
                Title = "Run Disk Cleanup",
                Description = "Temporary files, cache, and system logs accumulate over time and waste disk space.",
                Action = "Use Windows Disk Cleanup utility or Storage Sense to remove unnecessary files and free up space.",
                Priority = RecommendationPriority.Low,
                Category = "Maintenance",
                EstimatedImpact = 10
            });

            recommendations.Add(new Recommendation
            {
                Title = "Update Windows and Drivers",
                Description = "Keeping your system updated ensures security, stability, and optimal performance.",
                Action = "Check for Windows updates and update device drivers, especially graphics and chipset drivers.",
                Priority = RecommendationPriority.Medium,
                Category = "Maintenance",
                EstimatedImpact = 20
            });

            if (metrics.ProcessCount > 300)
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Manage Startup Programs",
                    Description = $"You have {metrics.ProcessCount} active processes. Many may be unnecessary startup programs.",
                    Action = "Open Task Manager > Startup tab and disable programs you don't need running at boot.",
                    Priority = RecommendationPriority.Medium,
                    Category = "Maintenance",
                    EstimatedImpact = 25
                });
            }

            return recommendations;
        }

        private List<Recommendation> GenerateDiagnosticRecommendations(
            IEnumerable<DiagnosticResult> diagnostics)
        {
            var recommendations = new List<Recommendation>();

            var criticalIssues = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Critical).ToList();
            if (criticalIssues.Any())
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Address Critical Issues Immediately",
                    Description = $"Found {criticalIssues.Count} critical issue(s) requiring immediate attention to prevent system instability.",
                    Action = "Review the Diagnostics tab and follow all recommended resolutions for critical issues.",
                    Priority = RecommendationPriority.Critical,
                    Category = "Diagnostics",
                    EstimatedImpact = 50
                });
            }

            var errorIssues = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errorIssues.Any())
            {
                recommendations.Add(new Recommendation
                {
                    Title = "Resolve Error Conditions",
                    Description = $"Found {errorIssues.Count} error(s) that may impact system performance and reliability.",
                    Action = "Check the Diagnostics tab and address all error-level issues to improve system health.",
                    Priority = RecommendationPriority.High,
                    Category = "Diagnostics",
                    EstimatedImpact = 30
                });
            }

            return recommendations;
        }

        private List<Recommendation> GenerateOptimizationRecommendations(
            SystemInfo systemInfo,
            PerformanceMetrics metrics)
        {
            var recommendations = new List<Recommendation>();

            recommendations.Add(new Recommendation
            {
                Title = "Enable Windows Performance Mode",
                Description = "Windows has power-saving features that can limit performance.",
                Action = "Go to Settings > System > Power > Power mode and select 'Best performance' for maximum speed.",
                Priority = RecommendationPriority.Low,
                Category = "Optimization",
                EstimatedImpact = 10
            });

            recommendations.Add(new Recommendation
            {
                Title = "Disable Visual Effects",
                Description = "Windows visual effects consume system resources that could be used for applications.",
                Action = "Search 'Performance Options', click 'Adjust for best performance', or selectively disable animations.",
                Priority = RecommendationPriority.Low,
                Category = "Optimization",
                EstimatedImpact = 8
            });

            recommendations.Add(new Recommendation
            {
                Title = "Defragment HDD (if applicable)",
                Description = "Fragmented hard drives slow down file access and system performance.",
                Action = "Run 'Defragment and Optimize Drives' utility for any HDDs (not needed for SSDs).",
                Priority = RecommendationPriority.Low,
                Category = "Optimization",
                EstimatedImpact = 12
            });

            return recommendations;
        }
    }
}