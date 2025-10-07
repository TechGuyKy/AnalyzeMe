using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AnalyzeMe.ViewModels;

namespace AnalyzeMe.Views
{
    public partial class PerformanceView : Page
    {
        private MainViewModel ViewModel => (MainViewModel)Application.Current.MainWindow.DataContext;
        private readonly DispatcherTimer _updateTimer;

        public PerformanceView()
        {
            InitializeComponent();
            DataContext = ViewModel;

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            Loaded += PerformanceView_Loaded;
        }

        private void PerformanceView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (ViewModel.CurrentMetrics == null) return;

            CpuPercentText.Text = $"{ViewModel.CurrentMetrics.CPUUsage:F1}%";
            CpuProgressBar.Value = ViewModel.CurrentMetrics.CPUUsage;
            UpdateStatusText(CpuStatusText, ViewModel.CurrentMetrics.CPUUsage, "CPU");
            RamPercentText.Text = $"{ViewModel.CurrentMetrics.RAMUsage:F1}%";
            RamProgressBar.Value = ViewModel.CurrentMetrics.RAMUsage;
            UpdateStatusText(RamStatusText, ViewModel.CurrentMetrics.RAMUsage, "Memory");
            DiskPercentText.Text = $"{ViewModel.CurrentMetrics.DiskUsage:F1}%";
            DiskProgressBar.Value = ViewModel.CurrentMetrics.DiskUsage;
            UpdateStatusText(DiskStatusText, ViewModel.CurrentMetrics.DiskUsage, "Disk");
            PageFilePercentText.Text = $"{ViewModel.CurrentMetrics.PageFileUsage:F1}%";
            PageFileProgressBar.Value = ViewModel.CurrentMetrics.PageFileUsage;
            UpdateStatusText(PageFileStatusText, ViewModel.CurrentMetrics.PageFileUsage, "Page File");
            ProcessCountText.Text = ViewModel.CurrentMetrics.ProcessCount.ToString();
            ThreadCountText.Text = ViewModel.CurrentMetrics.ThreadCount.ToString();
            HandleCountText.Text = ViewModel.CurrentMetrics.HandleCount.ToString();
            ProcessListView.ItemsSource = ViewModel.CurrentMetrics.TopProcesses;
        }

        private void UpdateStatusText(TextBlock textBlock, double usage, string resourceName)
        {
            if (usage > 90)
            {
                textBlock.Text = $"⚠️ Critical - {resourceName} usage is very high";
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            else if (usage > 75)
            {
                textBlock.Text = $"⚠️ Warning - {resourceName} usage is high";
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 170, 0));
            }
            else if (usage > 50)
            {
                textBlock.Text = $"✓ Moderate - {resourceName} usage is normal";
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 255));
            }
            else
            {
                textBlock.Text = $"✓ Good - {resourceName} usage is low";
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            }
        }
    }
}