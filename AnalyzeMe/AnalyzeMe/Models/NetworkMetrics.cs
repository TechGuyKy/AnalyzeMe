using System;

namespace AnalyzeMe.Models
{
    public class NetworkMetrics
    {
        public double DownloadSpeed { get; set; } //This is set for Mbps
        public double UploadSpeed { get; set; } //I also set this for Mbps
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public double TotalDownloadMB { get; set; }
        public double TotalUploadMB { get; set; }
        public string? ActiveAdapter { get; set; }
        public string? ConnectionType { get; set; }
        public bool IsConnected { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}