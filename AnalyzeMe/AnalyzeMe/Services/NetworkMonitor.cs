using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AnalyzeMe.Models;

namespace AnalyzeMe.Services
{
    public class NetworkMonitor
    {
        private PerformanceCounter? _downloadCounter;
        private PerformanceCounter? _uploadCounter;
        private long _sessionStartDownload = 0;
        private long _sessionStartUpload = 0;
        private DateTime _sessionStart = DateTime.MinValue;
        private string? _interfaceName = null;
        private bool _countersInitialized = false;

        //Added this for fallback speed calculations
        private long _lastBytesReceived = 0;
        private long _lastBytesSent = 0;
        private DateTime _lastCheck = DateTime.MinValue;

        public NetworkMonitor()
        {
            InitializePerformanceCounters();
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                Debug.WriteLine("Initializing Network Performance Counters...");

                //actively gets all network interfaces
                var activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                               ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                               ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .OrderByDescending(ni => ni.GetIPv4Statistics().BytesReceived)
                    .FirstOrDefault();

                if (activeInterface == null)
                {
                    Debug.WriteLine("No active network interface found");
                    return;
                }

                _interfaceName = activeInterface.Name;
                Debug.WriteLine($"Active interface: {_interfaceName}");

                //obtains all available performance counter interfaces
                var category = new PerformanceCounterCategory("Network Interface");
                var instanceNames = category.GetInstanceNames();

                Debug.WriteLine($"Available instances: {string.Join(", ", instanceNames)}");

                //trying a combination of strategies
                string? matchingInstance = null;

                // Strategy 1: Exact match for network interface
                matchingInstance = instanceNames.FirstOrDefault(name =>
                    name.Equals(_interfaceName, StringComparison.OrdinalIgnoreCase));

                // Strategy 2: Contains data pertaining to exact match
                if (matchingInstance == null)
                {
                    matchingInstance = instanceNames.FirstOrDefault(name =>
                        name.Contains(_interfaceName, StringComparison.OrdinalIgnoreCase) ||
                        _interfaceName.Contains(name, StringComparison.OrdinalIgnoreCase));
                }

                // Strategy 3: removes any special characters and tries again
                if (matchingInstance == null)
                {
                    var cleanInterfaceName = _interfaceName.Replace("(", "").Replace(")", "").Replace("#", "").Replace("_", " ").Trim();
                    matchingInstance = instanceNames.FirstOrDefault(name =>
                    {
                        var cleanName = name.Replace("(", "[").Replace(")", "]").Replace("#", "").Replace("_", " ").Trim();
                        return cleanName.Contains(cleanInterfaceName, StringComparison.OrdinalIgnoreCase) ||
                               cleanInterfaceName.Contains(cleanName, StringComparison.OrdinalIgnoreCase);
                    });
                }

                // Strategy 4: attempt to get the first wi-fi or ethernet adapter
                if (matchingInstance == null)
                {
                    matchingInstance = instanceNames.FirstOrDefault(name =>
                        name.Contains("Ethernet", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Wi-Fi", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Wireless", StringComparison.OrdinalIgnoreCase));
                }

                if (matchingInstance != null)
                {
                    Debug.WriteLine($"Using performance counter instance: {matchingInstance}");

                    _downloadCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", matchingInstance, true);
                    _uploadCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", matchingInstance, true);

                    //DO NOT ALTER: Must call NextValue() and wait before getting real values
                    _downloadCounter.NextValue();
                    _uploadCounter.NextValue();

                    _countersInitialized = true;
                    Debug.WriteLine("Performance counters initialized successfully");

                    //this initializes the current session tracking
                    var stats = activeInterface.GetIPv4Statistics();
                    _sessionStartDownload = stats.BytesReceived;
                    _sessionStartUpload = stats.BytesSent;
                    _sessionStart = DateTime.Now;

                    _lastBytesReceived = stats.BytesReceived;
                    _lastBytesSent = stats.BytesSent;
                    _lastCheck = DateTime.Now;
                }
                else
                {
                    Debug.WriteLine("Could not find matching performance counter instance");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing performance counters: {ex.Message}");
                _countersInitialized = false;
            }
        }

        public async Task<NetworkMetrics> GetNetworkMetricsAsync()
        {
            return await Task.Run(() =>
            {
                var metrics = new NetworkMetrics();

                try
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                        .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                   ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                   ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                        .ToList();

                    if (!interfaces.Any())
                    {
                        metrics.IsConnected = false;
                        metrics.ActiveAdapter = "No active connection";
                        return metrics;
                    }

                    //get the most active interface and set as default
                    var activeInterface = interfaces.OrderByDescending(ni =>
                        ni.GetIPv4Statistics().BytesReceived).First();

                    metrics.ActiveAdapter = activeInterface.Name;
                    metrics.ConnectionType = activeInterface.NetworkInterfaceType.ToString()
                        .Replace("Wireless80211", "Wi-Fi")
                        .Replace("Ethernet", "Ethernet");
                    metrics.IsConnected = true;

                    var stats = activeInterface.GetIPv4Statistics();

                    //Session totals start to finish
                    if (_sessionStart == DateTime.MinValue)
                    {
                        _sessionStartDownload = stats.BytesReceived;
                        _sessionStartUpload = stats.BytesSent;
                        _sessionStart = DateTime.Now;
                        _lastBytesReceived = stats.BytesReceived;
                        _lastBytesSent = stats.BytesSent;
                        _lastCheck = DateTime.Now;
                    }

                    metrics.TotalDownloadMB = (stats.BytesReceived - _sessionStartDownload) / (1024.0 * 1024.0);
                    metrics.TotalUploadMB = (stats.BytesSent - _sessionStartUpload) / (1024.0 * 1024.0);

                    //this calculates real-time speed - try performance counters first
                    bool speedCalculated = false;

                    if (_countersInitialized && _downloadCounter != null && _uploadCounter != null)
                    {
                        try
                        {
                            var downloadBytesPerSec = _downloadCounter.NextValue();
                            var uploadBytesPerSec = _uploadCounter.NextValue();

                            //conversion of bp/s to Mbp/s
                            metrics.DownloadSpeed = (downloadBytesPerSec * 8.0) / 1_000_000.0;
                            metrics.UploadSpeed = (uploadBytesPerSec * 8.0) / 1_000_000.0;

                            speedCalculated = true;

                            Debug.WriteLine($"Download: {metrics.DownloadSpeed:F2} Mbps, Upload: {metrics.UploadSpeed:F2} Mbps");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Performance counter error: {ex.Message}");
                            _countersInitialized = false;
                        }
                    }

                    //Fallback: If for some reason it fails to calculate it automatically, this manually calculates speed
                    if (!speedCalculated && _lastCheck != DateTime.MinValue)
                    {
                        var timeDiff = (DateTime.Now - _lastCheck).TotalSeconds;

                        if (timeDiff >= 0.5) //Only calculate if at least half a second has passed
                        {
                            var downloadDiff = stats.BytesReceived - _lastBytesReceived;
                            var uploadDiff = stats.BytesSent - _lastBytesSent;

                            //calculate the speed in Mbps
                            metrics.DownloadSpeed = (downloadDiff * 8.0) / (timeDiff * 1_000_000.0);
                            metrics.UploadSpeed = (uploadDiff * 8.0) / (timeDiff * 1_000_000.0);

                            _lastBytesReceived = stats.BytesReceived;
                            _lastBytesSent = stats.BytesSent;
                            _lastCheck = DateTime.Now;

                            Debug.WriteLine($"Fallback - Download: {metrics.DownloadSpeed:F2} Mbps, Upload: {metrics.UploadSpeed:F2} Mbps");
                        }
                    }
                    else if (_lastCheck == DateTime.MinValue)
                    {
                        //If first run, initialize
                        _lastBytesReceived = stats.BytesReceived;
                        _lastBytesSent = stats.BytesSent;
                        _lastCheck = DateTime.Now;
                    }

                    //Make sure there's no non negative values
                    metrics.DownloadSpeed = Math.Max(0, metrics.DownloadSpeed);
                    metrics.UploadSpeed = Math.Max(0, metrics.UploadSpeed);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting network metrics: {ex.Message}");
                    metrics.IsConnected = false;
                }

                return metrics;
            });
        }

        public void Reset()
        {
            Debug.WriteLine("Resetting network monitor...");

            _sessionStartDownload = 0;
            _sessionStartUpload = 0;
            _sessionStart = DateTime.MinValue;
            _lastBytesReceived = 0;
            _lastBytesSent = 0;
            _lastCheck = DateTime.MinValue;

            _downloadCounter?.Dispose();
            _uploadCounter?.Dispose();
            _downloadCounter = null;
            _uploadCounter = null;

            _countersInitialized = false;
            InitializePerformanceCounters();
        }

        public void Dispose()
        {
            _downloadCounter?.Dispose();
            _uploadCounter?.Dispose();
        }
    }
}