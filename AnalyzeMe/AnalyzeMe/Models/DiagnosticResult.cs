using System;

namespace AnalyzeMe.Models
{
    public enum DiagnosticSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public class DiagnosticResult
    {
        public string? Category { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DiagnosticSeverity Severity { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; }
        public string? Resolution { get; set; }

        public string SeverityIcon => Severity switch
        {
            DiagnosticSeverity.Info => "ℹ️",
            DiagnosticSeverity.Warning => "⚠️",
            DiagnosticSeverity.Error => "❌",
            DiagnosticSeverity.Critical => "🔥",
            _ => "•"
        };

        public string SeverityColor => Severity switch
        {
            DiagnosticSeverity.Info => "#00ffff",
            DiagnosticSeverity.Warning => "#ffaa00",
            DiagnosticSeverity.Error => "#ff0055",
            DiagnosticSeverity.Critical => "#ff0000",
            _ => "#6a7a8a"
        };
    }
}