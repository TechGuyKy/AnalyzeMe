using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AnalyzeMe.ViewModels;

namespace AnalyzeMe.Views
{
    public partial class DashboardView : Page
    {
        private MainViewModel ViewModel => (MainViewModel)Application.Current.MainWindow.DataContext;
        private readonly DispatcherTimer _updateTimer;

        public DashboardView()
        {
            InitializeComponent();
            DataContext = ViewModel;

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            Loaded += DashboardView_Loaded;
        }

        private void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (ViewModel.CurrentMetrics != null)
            {
                CpuUsageText.Text = $"{ViewModel.CurrentMetrics.CPUUsage:F1}%";
                RamUsageText.Text = $"{ViewModel.CurrentMetrics.RAMUsage:F1}%";
                DiskUsageText.Text = $"{ViewModel.CurrentMetrics.DiskUsage:F1}%";
                ProcessCountText.Text = ViewModel.CurrentMetrics.ProcessCount.ToString();
            }

            if (ViewModel.SystemInfo != null)
            {
                ComputerNameText.Text = ViewModel.SystemInfo.ComputerName;
                OsText.Text = ViewModel.SystemInfo.OperatingSystem;
                ProcessorText.Text = ViewModel.SystemInfo.ProcessorName;
                CpuSpeedText.Text = $"{ViewModel.SystemInfo.ProcessorBaseSpeed:F2} GHz (Max: {ViewModel.SystemInfo.ProcessorMaxSpeed:F2} GHz)";
                TotalRamText.Text = $"{ViewModel.SystemInfo.TotalRAM:F1} GB ({ViewModel.SystemInfo.MemorySlotsUsed}/{ViewModel.SystemInfo.MemorySlots} slots)";
                MemoryTypeText.Text = $"{ViewModel.SystemInfo.MemoryType} @ {ViewModel.SystemInfo.MemorySpeed}";
                GraphicsText.Text = ViewModel.SystemInfo.GraphicsCard;
                UptimeText.Text = $"{ViewModel.SystemInfo.Uptime.Days}d {ViewModel.SystemInfo.Uptime.Hours}h {ViewModel.SystemInfo.Uptime.Minutes}m";

                //function to calculate system healthscore
                var healthScore = CalculateHealthScore();
                HealthScoreText.Text = healthScore.ToString();

                if (healthScore >= 80)
                {
                    HealthStatusText.Text = "EXCELLENT";
                    HealthStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                    HealthScoreText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                }
                else if (healthScore >= 60)
                {
                    HealthStatusText.Text = "GOOD";
                    HealthStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 255));
                    HealthScoreText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 255));
                }
                else if (healthScore >= 40)
                {
                    HealthStatusText.Text = "FAIR";
                    HealthStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 170, 0));
                    HealthScoreText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 170, 0));
                }
                else
                {
                    HealthStatusText.Text = "POOR";
                    HealthStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                    HealthScoreText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                }
            }
        }

        private int CalculateHealthScore()
        {
            int score = 100;

            if (ViewModel.CurrentMetrics != null)
            {
                //reduce points for high cpu usage
                if (ViewModel.CurrentMetrics.CPUUsage > 90) score -= 20;
                else if (ViewModel.CurrentMetrics.CPUUsage > 75) score -= 10;
                else if (ViewModel.CurrentMetrics.CPUUsage > 50) score -= 5;

                //reduce points for high RAM usage
                if (ViewModel.CurrentMetrics.RAMUsage > 90) score -= 20;
                else if (ViewModel.CurrentMetrics.RAMUsage > 75) score -= 10;
                else if (ViewModel.CurrentMetrics.RAMUsage > 50) score -= 5;

                //reduce points for high disk usage
                if (ViewModel.CurrentMetrics.DiskUsage > 95) score -= 15;
                else if (ViewModel.CurrentMetrics.DiskUsage > 85) score -= 8;
            }

            if (ViewModel.SystemInfo != null)
            {
                //reduce points for low RAM
                if (ViewModel.SystemInfo.TotalRAM < 8) score -= 15;
                else if (ViewModel.SystemInfo.TotalRAM < 16) score -= 5;

                //reduce points for outdated/slow CPU
                if (ViewModel.SystemInfo.ProcessorCores < 4) score -= 10;
            }

            //reduce points for true diagnostic issues
            var criticalCount = 0;
            var errorCount = 0;
            foreach (var diag in ViewModel.DiagnosticResults)
            {
                if (diag.Severity == Models.DiagnosticSeverity.Critical) criticalCount++;
                if (diag.Severity == Models.DiagnosticSeverity.Error) errorCount++;
            }
            score -= (criticalCount * 10);
            score -= (errorCount * 5);

            return Math.Max(0, Math.Min(100, score));
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.RunDiagnosticsAsync();
            await ViewModel.GenerateRecommendationsAsync();
            MessageBox.Show("System data refreshed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DiagnosticsButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.RunDiagnosticsAsync();
            MessageBox.Show($"Diagnostics complete! Found {ViewModel.DiagnosticResults.Count} result(s).\n\nCheck the Diagnostics tab for details.",
                "Diagnostics Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void RecommendationsButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.GenerateRecommendationsAsync();
            MessageBox.Show($"Generated {ViewModel.Recommendations.Count} optimization recommendation(s).\n\nCheck the Optimize tab for details.",
                "Recommendations Ready", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}