using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AnalyzeMe.Models;
using AnalyzeMe.Services;

namespace AnalyzeMe.Views
{
    public partial class ManagerView : Page
    {
        private readonly ProcessManager _processManager;
        private readonly DispatcherTimer _updateTimer;
        private ObservableCollection<TaskProcessInfo> _processes;
        private TaskProcessInfo? _selectedProcess;

        public ManagerView()
        {
            InitializeComponent();

            _processManager = new ProcessManager();
            _processes = new ObservableCollection<TaskProcessInfo>();
            ProcessListView.ItemsSource = _processes;

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _updateTimer.Tick += async (s, e) => await LoadProcessesAsync();

            Loaded += async (s, e) => { await LoadProcessesAsync(); _updateTimer.Start(); };
        }

        private async System.Threading.Tasks.Task LoadProcessesAsync()
        {
            try
            {
                var allProcesses = await _processManager.GetRunningProcessesAsync();

                ProcessCountText.Text = allProcesses.Count.ToString();
                ThreadCountText.Text = allProcesses.Sum(p => p.ThreadCount).ToString();
                HandleCountText.Text = allProcesses.Sum(p => p.HandleCount).ToString();
                TotalRamUsedText.Text = $"{allProcesses.Sum(p => p.MemoryMB) / 1024.0:F2} GB";

                var searchText = SearchBox.Text.ToLower();
                var filtered = string.IsNullOrWhiteSpace(searchText)
                    ? allProcesses
                    : allProcesses.Where(p => p.Name?.ToLower().Contains(searchText) == true).ToList();

                _processes.Clear();
                foreach (var process in filtered)
                    _processes.Add(process);
            }
            catch { }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => _ = LoadProcessesAsync();

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _processManager.ClearCpuCache();
            await LoadProcessesAsync();
        }

        private void ProcessListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProcess = ProcessListView.SelectedItem as TaskProcessInfo;
            EndTaskButton.IsEnabled = _selectedProcess != null;
        }

        private void ProcessListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedProcess != null)
                ShowProcessProperties(_selectedProcess);
        }

        private async void EndTaskButton_Click(object sender, RoutedEventArgs e)
            => await EndSelectedProcess();

        private async void EndTaskMenuItem_Click(object sender, RoutedEventArgs e)
            => await EndSelectedProcess();

        private async System.Threading.Tasks.Task EndSelectedProcess()
        {
            if (_selectedProcess == null) return;

            if (MessageBox.Show($"End '{_selectedProcess.Name}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (await _processManager.KillProcessAsync(_selectedProcess.ProcessId))
                {
                    MessageBox.Show("Process terminated.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadProcessesAsync();
                }
                else
                    MessageBox.Show("Failed to terminate process.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SuspendMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProcess != null)
                await _processManager.SuspendProcessAsync(_selectedProcess.ProcessId);
        }

        private async void ResumeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProcess != null)
                await _processManager.ResumeProcessAsync(_selectedProcess.ProcessId);
        }

        private async void SetPriorityMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProcess != null && sender is MenuItem item && item.Tag is string priority)
            {
                await _processManager.SetProcessPriorityAsync(_selectedProcess.ProcessId, priority);
                await LoadProcessesAsync();
            }
        }

        private void OpenLocationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProcess?.FilePath != null)
                Process.Start("explorer.exe", $"/select,\"{_selectedProcess.FilePath}\"");
        }

        private void PropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProcess != null)
                ShowProcessProperties(_selectedProcess);
        }

        private void ShowProcessProperties(TaskProcessInfo p)
        {
            MessageBox.Show(
                $"Name: {p.Name}\nPID: {p.ProcessId}\nCPU: {p.CpuUsageDisplay}\n" +
                $"Memory: {p.MemoryDisplay}\nThreads: {p.ThreadCount}\nPriority: {p.PriorityClass}\n" +
                $"Path: {p.FilePath ?? "N/A"}",
                "Process Properties", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}