using System;
using System.Collections.Generic;

namespace AnalyzeMe.Models
{
    public class PerformanceMetrics
    {
        public double CPUUsage { get; set; }
        public double RAMUsage { get; set; }
        public double DiskUsage { get; set; }
        public double NetworkUsage { get; set; }
        public int ProcessCount { get; set; }
        public int ThreadCount { get; set; }
        public double CPUTemperature { get; set; }
        public int HandleCount { get; set; }
        public double PageFileUsage { get; set; }
        public List<ProcessInfo> TopProcesses { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ProcessInfo
    {
        public string? Name { get; set; }
        public int Id { get; set; }
        public double CPUUsage { get; set; }
        public double MemoryUsage { get; set; }
        public string? Status { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }

        public string DisplayMemory => $"{MemoryUsage:F1} MB";
        public string DisplayCPU => $"{CPUUsage:F1}%";
    }
}