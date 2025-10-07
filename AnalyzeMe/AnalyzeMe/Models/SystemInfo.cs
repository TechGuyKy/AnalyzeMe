using System;
using System.Collections.Generic;

namespace AnalyzeMe.Models
{
    public class SystemInfo
    {
        public string? ComputerName { get; set; }
        public string? OperatingSystem { get; set; }
        public string? WindowsVersion { get; set; }
        public string? SystemArchitecture { get; set; }
        public string? ProcessorName { get; set; }
        public int ProcessorCores { get; set; }
        public int ProcessorThreads { get; set; }
        public double ProcessorBaseSpeed { get; set; }
        public double ProcessorMaxSpeed { get; set; }
        public double TotalRAM { get; set; }
        public double AvailableRAM { get; set; }
        public string? MemorySpeed { get; set; }
        public string? MemoryType { get; set; }
        public int MemorySlots { get; set; }
        public int MemorySlotsUsed { get; set; }
        public string? GraphicsCard { get; set; }
        public double GraphicsMemory { get; set; }
        public string? MotherboardName { get; set; }
        public string? BiosVersion { get; set; }
        public string? BiosDate { get; set; }
        public List<DiskInfo> Disks { get; set; } = new();
        public double TotalDiskSpace { get; set; }
        public double FreeDiskSpace { get; set; }
        public DateTime LastBootTime { get; set; }
        public TimeSpan Uptime => DateTime.Now - LastBootTime;

        public double RAMUsagePercentage =>
            TotalRAM > 0 ? ((TotalRAM - AvailableRAM) / TotalRAM) * 100 : 0;

        public double DiskUsagePercentage =>
            TotalDiskSpace > 0 ? ((TotalDiskSpace - FreeDiskSpace) / TotalDiskSpace) * 100 : 0;
    }

    public class DiskInfo
    {
        public string Model { get; set; } = "";
        public double Size { get; set; }
        public string InterfaceType { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string MediaType { get; set; } = "";
        public int Partitions { get; set; }

        public string DisplaySize => $"{Size:F0} GB";
        public string DisplayType => MediaType == "SSD" ? "🚀 SSD" : "💾 HDD";
    }
}