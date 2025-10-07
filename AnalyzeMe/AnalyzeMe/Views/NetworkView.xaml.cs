using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AnalyzeMe.Services;

namespace AnalyzeMe.Views
{
    public partial class NetworkView : Page
    {
        private readonly SpeedTestService _speedTestService;
        private DispatcherTimer? _monitorTimer;
        private NetworkInterface? _selectedAdapter;
        private long _sessionStartDownload = 0;
        private long _sessionStartUpload = 0;
        private long _previousBytesReceived = 0;
        private long _previousBytesSent = 0;
        private DateTime _previousCheck = DateTime.MinValue;
        private double _maxDownloadSpeed = 100;
        private double _maxUploadSpeed = 50;
        private bool _isSpeedTestRunning = false;
        private int _updateCount = 0;

        public NetworkView()
        {
            InitializeComponent();
            _speedTestService = new SpeedTestService();
            Loaded += NetworkView_Loaded;
        }

        private async void NetworkView_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if Speedtest CLI is available
            if (!_speedTestService.IsSpeedTestAvailable())
            {
                SpeedTestButton.IsEnabled = false;
                SpeedTestButton.Content = "❌ SPEEDTEST NOT FOUND";
                SpeedTestDownloadStatusText.Text = "Download speedtest.exe from speedtest.net/apps/cli";
                SpeedTestUploadStatusText.Text = "Place it in the Tools folder and restart";
                SpeedTestPingText.Text = "-- ms";
                SpeedTestServerText.Text = "Speedtest CLI required";
            }

            LoadNetworkAdapters();
        }

        private void LoadNetworkAdapters()
        {
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                               ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                               !ni.Name.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase) &&
                               !ni.Description.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase))
                    .Select(ni => new NetworkAdapterInfo
                    {
                        Interface = ni,
                        Name = ni.Name,
                        Description = ni.Description,
                        Type = GetAdapterTypeDisplay(ni.NetworkInterfaceType),
                        Speed = ni.Speed > 0 ? $"{ni.Speed / 1_000_000} Mbps" : "Unknown",
                        Status = ni.OperationalStatus.ToString(),
                        StatusColor = GetStatusColor(ni.OperationalStatus),
                        Icon = GetAdapterIcon(ni.NetworkInterfaceType, ni.OperationalStatus)
                    })
                    .OrderByDescending(a => a.Interface.OperationalStatus == OperationalStatus.Up)
                    .ThenByDescending(a => a.Interface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    .ToList();

                AdapterListView.ItemsSource = adapters;

                // Auto-select first active adapter
                var firstActive = adapters.FirstOrDefault(a => a.Interface.OperationalStatus == OperationalStatus.Up);
                if (firstActive != null)
                {
                    AdapterListView.SelectedItem = firstActive;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading adapters: {ex.Message}");
                MessageBox.Show($"Error loading network adapters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetAdapterTypeDisplay(NetworkInterfaceType type)
        {
            return type switch
            {
                NetworkInterfaceType.Ethernet => "Ethernet",
                NetworkInterfaceType.Wireless80211 => "Wi-Fi",
                NetworkInterfaceType.Ppp => "PPP",
                NetworkInterfaceType.TokenRing => "Token Ring",
                NetworkInterfaceType.Fddi => "FDDI",
                NetworkInterfaceType.Loopback => "Loopback",
                _ => type.ToString()
            };
        }

        private SolidColorBrush GetStatusColor(OperationalStatus status)
        {
            return status switch
            {
                OperationalStatus.Up => new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                OperationalStatus.Down => new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                OperationalStatus.Testing => new SolidColorBrush(Color.FromRgb(255, 170, 0)),
                OperationalStatus.Dormant => new SolidColorBrush(Color.FromRgb(106, 122, 138)),
                _ => new SolidColorBrush(Color.FromRgb(106, 122, 138))
            };
        }

        private string GetAdapterIcon(NetworkInterfaceType type, OperationalStatus status)
        {
            if (status != OperationalStatus.Up)
                return "⚠️";

            return type switch
            {
                NetworkInterfaceType.Ethernet => "🔌",
                NetworkInterfaceType.Wireless80211 => "📡",
                NetworkInterfaceType.Ppp => "📞",
                _ => "🌐"
            };
        }

        private void AdapterListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AdapterListView.SelectedItem is NetworkAdapterInfo adapterInfo)
            {
                SelectAdapter(adapterInfo.Interface);
            }
        }

        private void SelectAdapter(NetworkInterface adapter)
        {
            try
            {
                // Stop existing monitoring
                StopMonitoring();

                _selectedAdapter = adapter;

                // Reset metrics
                var stats = adapter.GetIPv4Statistics();
                _sessionStartDownload = stats.BytesReceived;
                _sessionStartUpload = stats.BytesSent;
                _previousBytesReceived = stats.BytesReceived;
                _previousBytesSent = stats.BytesSent;
                _previousCheck = DateTime.Now;
                _updateCount = 0;

                // Update UI
                SelectedAdapterText.Text = $"✓ Monitoring: {adapter.Name}";
                SelectedAdapterText.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));

                ConnectionStatusText.Text = adapter.OperationalStatus.ToString();
                ConnectionStatusText.Foreground = adapter.OperationalStatus == OperationalStatus.Up
                    ? new SolidColorBrush(Color.FromRgb(0, 255, 0))
                    : new SolidColorBrush(Color.FromRgb(255, 0, 0));

                AdapterNameText.Text = adapter.Description;
                ConnectionTypeText.Text = GetAdapterTypeDisplay(adapter.NetworkInterfaceType);

                // Reset displays
                DownloadSpeedText.Text = "0.00 Mbps";
                UploadSpeedText.Text = "0.00 Mbps";
                TotalDownloadText.Text = "0.0 MB";
                TotalUploadText.Text = "0.0 MB";
                DownloadProgressBar.Value = 0;
                UploadProgressBar.Value = 0;
                DownloadStatusText.Text = "💤 Idle";
                UploadStatusText.Text = "💤 Idle";

                // Start monitoring
                StartMonitoring();

                Debug.WriteLine($"Now monitoring: {adapter.Name} ({adapter.Description})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error selecting adapter: {ex.Message}");
                MessageBox.Show($"Error selecting adapter: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartMonitoring()
        {
            _monitorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _monitorTimer.Tick += MonitorTimer_Tick;
            _monitorTimer.Start();

            Debug.WriteLine("Network monitoring started");
        }

        private void StopMonitoring()
        {
            if (_monitorTimer != null)
            {
                _monitorTimer.Stop();
                _monitorTimer.Tick -= MonitorTimer_Tick;
                _monitorTimer = null;
                Debug.WriteLine("Network monitoring stopped");
            }
        }

        private void MonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (_selectedAdapter == null || _isSpeedTestRunning)
                return;

            try
            {
                var currentStats = _selectedAdapter.GetIPv4Statistics();
                var currentTime = DateTime.Now;

                // Calculate session totals
                var totalDownloadMB = (currentStats.BytesReceived - _sessionStartDownload) / (1024.0 * 1024.0);
                var totalUploadMB = (currentStats.BytesSent - _sessionStartUpload) / (1024.0 * 1024.0);

                // Format total transfer
                if (totalDownloadMB > 1024)
                    TotalDownloadText.Text = $"{totalDownloadMB / 1024:F2} GB";
                else if (totalDownloadMB > 1)
                    TotalDownloadText.Text = $"{totalDownloadMB:F1} MB";
                else
                    TotalDownloadText.Text = $"{totalDownloadMB * 1024:F0} KB";

                if (totalUploadMB > 1024)
                    TotalUploadText.Text = $"{totalUploadMB / 1024:F2} GB";
                else if (totalUploadMB > 1)
                    TotalUploadText.Text = $"{totalUploadMB:F1} MB";
                else
                    TotalUploadText.Text = $"{totalUploadMB * 1024:F0} KB";

                // Calculate real-time speed
                if (_previousCheck != DateTime.MinValue)
                {
                    var timeDiffSeconds = (currentTime - _previousCheck).TotalSeconds;

                    if (timeDiffSeconds > 0)
                    {
                        var downloadDiff = currentStats.BytesReceived - _previousBytesReceived;
                        var uploadDiff = currentStats.BytesSent - _previousBytesSent;

                        // Calculate speed in Mbps
                        var downloadSpeed = (downloadDiff * 8.0) / (timeDiffSeconds * 1_000_000.0);
                        var uploadSpeed = (uploadDiff * 8.0) / (timeDiffSeconds * 1_000_000.0);

                        DownloadSpeedText.Text = $"{downloadSpeed:F2} Mbps";
                        UploadSpeedText.Text = $"{uploadSpeed:F2} Mbps";

                        // Auto-scale progress bar maximum
                        if (downloadSpeed > _maxDownloadSpeed * 0.8)
                        {
                            _maxDownloadSpeed = Math.Max(100, downloadSpeed * 1.5);
                        }
                        else if (downloadSpeed < _maxDownloadSpeed * 0.1 && _maxDownloadSpeed > 100 && _updateCount > 10)
                        {
                            _maxDownloadSpeed = Math.Max(100, _maxDownloadSpeed * 0.7);
                        }

                        DownloadProgressBar.Maximum = _maxDownloadSpeed;
                        DownloadProgressBar.Value = Math.Min(downloadSpeed, _maxDownloadSpeed);

                        // Download status messages
                        if (downloadSpeed > 500)
                            DownloadStatusText.Text = "🚀 Gigabit Activity!";
                        else if (downloadSpeed > 100)
                            DownloadStatusText.Text = "⚡ Very High Activity";
                        else if (downloadSpeed > 10)
                            DownloadStatusText.Text = "✓ High Activity";
                        else if (downloadSpeed > 1)
                            DownloadStatusText.Text = "📊 Moderate Activity";
                        else if (downloadSpeed > 0.1)
                            DownloadStatusText.Text = "🔹 Low Activity";
                        else
                            DownloadStatusText.Text = "💤 Idle";

                        // Upload handling
                        if (uploadSpeed > _maxUploadSpeed * 0.8)
                        {
                            _maxUploadSpeed = Math.Max(50, uploadSpeed * 1.5);
                        }
                        else if (uploadSpeed < _maxUploadSpeed * 0.1 && _maxUploadSpeed > 50 && _updateCount > 10)
                        {
                            _maxUploadSpeed = Math.Max(50, _maxUploadSpeed * 0.7);
                        }

                        UploadProgressBar.Maximum = _maxUploadSpeed;
                        UploadProgressBar.Value = Math.Min(uploadSpeed, _maxUploadSpeed);

                        // Upload status messages
                        if (uploadSpeed > 200)
                            UploadStatusText.Text = "🚀 Ultra-Fast Upload!";
                        else if (uploadSpeed > 50)
                            UploadStatusText.Text = "⚡ Very High Activity";
                        else if (uploadSpeed > 5)
                            UploadStatusText.Text = "✓ High Activity";
                        else if (uploadSpeed > 0.5)
                            UploadStatusText.Text = "📊 Moderate Activity";
                        else if (uploadSpeed > 0.05)
                            UploadStatusText.Text = "🔹 Low Activity";
                        else
                            UploadStatusText.Text = "💤 Idle";

                        _updateCount++;
                    }
                }

                _previousBytesReceived = currentStats.BytesReceived;
                _previousBytesSent = currentStats.BytesSent;
                _previousCheck = currentTime;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in monitor tick: {ex.Message}");
            }
        }

        private void RefreshAdaptersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadNetworkAdapters();
        }

        private async void SpeedTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSpeedTestRunning) return;

            _isSpeedTestRunning = true;
            SpeedTestButton.IsEnabled = false;
            SpeedTestButton.Content = "⏳ TESTING...";
            SpeedTestProgressBar.Visibility = Visibility.Visible;
            SpeedTestProgressBar.IsIndeterminate = true;

            SpeedTestDownloadText.Text = "Testing...";
            SpeedTestUploadText.Text = "Testing...";
            SpeedTestPingText.Text = "Testing...";
            SpeedTestDownloadStatusText.Text = "Running Ookla Speedtest...";
            SpeedTestUploadStatusText.Text = "Please wait (30-60 seconds)...";
            SpeedTestServerText.Text = "Connecting to server...";

            try
            {
                var result = await _speedTestService.RunSpeedTestAsync();

                if (result.Success)
                {
                    // Download Speed
                    SpeedTestDownloadText.Text = $"{result.DownloadMbps:F2} Mbps";
                    SpeedTestDownloadText.Foreground = result.DownloadMbps > 900
                        ? new SolidColorBrush(Color.FromRgb(0, 255, 0))
                        : result.DownloadMbps > 500
                        ? new SolidColorBrush(Color.FromRgb(0, 255, 255))
                        : result.DownloadMbps > 100
                        ? new SolidColorBrush(Color.FromRgb(255, 170, 0))
                        : new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    SpeedTestDownloadStatusText.Text = GetSpeedRating(result.DownloadMbps);

                    // Upload Speed
                    SpeedTestUploadText.Text = $"{result.UploadMbps:F2} Mbps";
                    SpeedTestUploadText.Foreground = result.UploadMbps > 500
                        ? new SolidColorBrush(Color.FromRgb(0, 255, 0))
                        : result.UploadMbps > 100
                        ? new SolidColorBrush(Color.FromRgb(255, 0, 255))
                        : result.UploadMbps > 50
                        ? new SolidColorBrush(Color.FromRgb(255, 170, 0))
                        : new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    SpeedTestUploadStatusText.Text = GetSpeedRating(result.UploadMbps);

                    // Ping
                    SpeedTestPingText.Text = $"{result.PingMs:F0} ms";
                    SpeedTestPingText.Foreground = result.PingMs < 20
                        ? new SolidColorBrush(Color.FromRgb(0, 255, 0))
                        : result.PingMs < 50
                        ? new SolidColorBrush(Color.FromRgb(0, 255, 255))
                        : result.PingMs < 100
                        ? new SolidColorBrush(Color.FromRgb(255, 170, 0))
                        : new SolidColorBrush(Color.FromRgb(255, 0, 0));

                    // Server Info
                    SpeedTestServerText.Text = $"{result.ServerName} ({result.ServerLocation}) • {result.Isp}";
                    SpeedTestServerText.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 255));
                }
                else
                {
                    throw new Exception(result.ErrorMessage ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                SpeedTestDownloadText.Text = "Error";
                SpeedTestUploadText.Text = "Error";
                SpeedTestPingText.Text = "Error";
                SpeedTestDownloadStatusText.Text = "Test failed";
                SpeedTestUploadStatusText.Text = ex.Message;
                SpeedTestServerText.Text = "Check Tools\\speedtest.exe";

                var errorColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                SpeedTestDownloadText.Foreground = errorColor;
                SpeedTestUploadText.Foreground = errorColor;
                SpeedTestPingText.Foreground = errorColor;
            }
            finally
            {
                SpeedTestProgressBar.IsIndeterminate = false;
                SpeedTestProgressBar.Visibility = Visibility.Collapsed;
                SpeedTestButton.IsEnabled = true;
                SpeedTestButton.Content = "🚀 RUN SPEED TEST";
                _isSpeedTestRunning = false;
            }
        }

        private string GetSpeedRating(double speedMbps)
        {
            if (speedMbps > 900) return "🚀 Gigabit+ - Exceptional!";
            if (speedMbps > 500) return "🚀 Ultra-fast - Outstanding!";
            if (speedMbps > 100) return "⚡ Excellent - Perfect!";
            if (speedMbps > 50) return "✓ Very Good - Great!";
            if (speedMbps > 25) return "✓ Good - HD ready";
            if (speedMbps > 10) return "⚠️ Fair - Basic";
            return "❌ Slow connection";
        }
    }

    public class NetworkAdapterInfo
    {
        public NetworkInterface Interface { get; set; } = null!;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "";
        public string Speed { get; set; } = "";
        public string Status { get; set; } = "";
        public SolidColorBrush StatusColor { get; set; } = new SolidColorBrush(Colors.Gray);
        public string Icon { get; set; } = "";
    }
}