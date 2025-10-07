using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnalyzeMe.Models
{
    public class TaskProcessInfo : INotifyPropertyChanged
    {
        private double _cpuUsage;
        private long _memoryMB;
        private string? _status;

        public int ProcessId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public string? FilePath { get; set; }

        public double CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CpuUsageDisplay));
            }
        }

        public long MemoryMB
        {
            get => _memoryMB;
            set
            {
                _memoryMB = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryDisplay));
            }
        }

        public long MemoryBytes { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public DateTime StartTime { get; set; }
        public string? UserName { get; set; }
        public string? PriorityClass { get; set; }

        public string? Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsSystemProcess { get; set; }
        public bool Responding { get; set; }

        // Display properties
        public string CpuUsageDisplay => $"{CpuUsage:F1}%";
        public string MemoryDisplay => MemoryMB > 1024 ? $"{MemoryMB / 1024.0:F2} GB" : $"{MemoryMB:F0} MB";
        public string StartTimeDisplay => StartTime.ToString("MM/dd/yyyy HH:mm:ss");
        public string UptimeDisplay
        {
            get
            {
                var uptime = DateTime.Now - StartTime;
                if (uptime.TotalDays >= 1)
                    return $"{(int)uptime.TotalDays}d {uptime.Hours}h";
                if (uptime.TotalHours >= 1)
                    return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
                return $"{(int)uptime.TotalMinutes}m {uptime.Seconds}s";
            }
        }

        public string StatusIcon => Responding ? "✓" : "⚠️";
        public string StatusColor => Responding ? "#00ff00" : "#ff0000";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}