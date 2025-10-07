using System;

namespace AnalyzeMe.Models
{
    public class ProgramInfo
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? Publisher { get; set; }
        public string? InstallDate { get; set; }
        public string? InstallLocation { get; set; }
        public string? UninstallString { get; set; }
        public double SizeMB { get; set; }
        public string DisplaySize => SizeMB > 0 ? $"{SizeMB:F1} MB" : "Unknown";
    }

    public class StartupProgram
    {
        public string? Name { get; set; }
        public string? Command { get; set; }
        public string? Location { get; set; }
        public bool IsEnabled { get; set; }
        public string Status => IsEnabled ? "Enabled" : "Disabled";
    }

    public class WindowsService
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Status { get; set; }
        public string? StartupType { get; set; }
        public string? Description { get; set; }
    }
}