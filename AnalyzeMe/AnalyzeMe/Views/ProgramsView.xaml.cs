using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AnalyzeMe.Models;
using AnalyzeMe.Services;

namespace AnalyzeMe.Views
{
    public partial class ProgramsView : Page
    {
        private readonly ProgramManager _programManager;
        private List<ProgramInfo> _allPrograms = new();
        private List<StartupProgram> _allStartupPrograms = new();
        private List<WindowsService> _allServices = new();
        private bool _isInitialized = false;

        public ProgramsView()
        {
            InitializeComponent();
            _programManager = new ProgramManager();

            //Had to mark as initialized BEFORE loading otherwise would throw error when navigating Programs tab
            _isInitialized = true;

            Loaded += ProgramsView_Loaded;
        }

        private async void ProgramsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            //Load the currently selected tab, within the tab.
            if (InstalledProgramsTab.IsChecked == true)
            {
                await LoadInstalledPrograms();
            }
        }

        private void TabButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            if (InstalledProgramsView == null) return;

            InstalledProgramsView.Visibility = Visibility.Collapsed;
            StartupProgramsView.Visibility = Visibility.Collapsed;
            ServicesView.Visibility = Visibility.Collapsed;

            if (sender == InstalledProgramsTab)
            {
                InstalledProgramsView.Visibility = Visibility.Visible;
                _ = LoadInstalledPrograms();
            }
            else if (sender == StartupProgramsTab)
            {
                StartupProgramsView.Visibility = Visibility.Visible;
                _ = LoadStartupPrograms();
            }
            else if (sender == ServicesTab)
            {
                ServicesView.Visibility = Visibility.Visible;
                _ = LoadServices();
            }
        }

        private async System.Threading.Tasks.Task LoadInstalledPrograms()
        {
            try
            {
                //Show loading state
                if (ProgramCountText != null)
                    ProgramCountText.Text = "Loading...";

                _allPrograms = await _programManager.GetInstalledProgramsAsync();

                if (ProgramsListView != null)
                    ProgramsListView.ItemsSource = _allPrograms;

                if (ProgramCountText != null)
                    ProgramCountText.Text = $"{_allPrograms.Count} programs";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading programs: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (ProgramCountText != null)
                    ProgramCountText.Text = "Error loading";
            }
        }

        private async System.Threading.Tasks.Task LoadStartupPrograms()
        {
            try
            {
                if (StartupCountText != null)
                    StartupCountText.Text = "Loading...";

                _allStartupPrograms = await _programManager.GetStartupProgramsAsync();

                if (StartupListView != null)
                    StartupListView.ItemsSource = _allStartupPrograms;

                if (StartupCountText != null)
                    StartupCountText.Text = $"{_allStartupPrograms.Count} programs";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading startup programs: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (StartupCountText != null)
                    StartupCountText.Text = "Error loading";
            }
        }

        private async System.Threading.Tasks.Task LoadServices()
        {
            try
            {
                if (ServiceCountText != null)
                    ServiceCountText.Text = "Loading...";

                _allServices = await _programManager.GetWindowsServicesAsync();

                if (ServicesListView != null)
                    ServicesListView.ItemsSource = _allServices;

                if (ServiceCountText != null)
                    ServiceCountText.Text = $"{_allServices.Count} services";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading services: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (ServiceCountText != null)
                    ServiceCountText.Text = "Error loading";
            }
        }

        private async void RefreshPrograms_Click(object sender, RoutedEventArgs e)
        {
            await LoadInstalledPrograms();
            MessageBox.Show("Programs list refreshed!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void RefreshStartup_Click(object sender, RoutedEventArgs e)
        {
            await LoadStartupPrograms();
            MessageBox.Show("Startup programs refreshed!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void RefreshServices_Click(object sender, RoutedEventArgs e)
        {
            await LoadServices();
            MessageBox.Show("Services list refreshed!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void UninstallProgram_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is not ProgramInfo program) return;

            var result = MessageBox.Show(
                $"Are you sure you want to uninstall:\n\n{program.Name}\n\nThis action cannot be undone.",
                "Confirm Uninstall",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                button.IsEnabled = false;
                button.Content = "⏳ UNINSTALLING...";

                var success = await _programManager.UninstallProgramAsync(program);

                if (success)
                {
                    MessageBox.Show($"{program.Name} uninstall initiated.\n\nFollow the uninstaller prompts to complete removal.",
                        "Uninstall Started", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadInstalledPrograms();
                }
                else
                {
                    MessageBox.Show($"Failed to uninstall {program.Name}.\n\nTry uninstalling from Windows Settings.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    button.IsEnabled = true;
                    button.Content = "🗑️ UNINSTALL";
                }
            }
        }

        private async void DisableStartup_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is not StartupProgram program) return;

            var result = MessageBox.Show(
                $"Disable '{program.Name}' from starting automatically with Windows?",
                "Confirm Disable",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                button.IsEnabled = false;
                button.Content = "⏳ DISABLING...";

                var success = await _programManager.DisableStartupProgramAsync(program);

                if (success)
                {
                    MessageBox.Show($"{program.Name} has been disabled from startup.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadStartupPrograms();
                }
                else
                {
                    MessageBox.Show($"Failed to disable {program.Name}.\n\nTry using Task Manager > Startup tab.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    button.IsEnabled = true;
                    button.Content = "🚫 DISABLE";
                }
            }
        }

        private async void ChangeServiceStartup_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is not WindowsService service) return;

            //Find the actual ComboBox in the same visual tree
            var parent = button.Parent as StackPanel;
            var comboBox = parent?.Children.OfType<ComboBox>().FirstOrDefault(cb => cb.Tag == service);

            if (comboBox?.SelectedItem is not ComboBoxItem selectedItem) return;

            var newStartupType = selectedItem.Content?.ToString();
            if (string.IsNullOrEmpty(newStartupType) || newStartupType == service.StartupType) return;

            var result = MessageBox.Show(
                $"Change '{service.DisplayName}' startup type to {newStartupType}?\n\n" +
                $"⚠️ WARNING: This may affect system functionality.",
                "Confirm Change",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                button.IsEnabled = false;
                var originalContent = button.Content;
                button.Content = "⏳ APPLYING...";

                var success = await _programManager.ChangeServiceStartupTypeAsync(service.Name ?? "", newStartupType);

                if (success)
                {
                    service.StartupType = newStartupType;
                    MessageBox.Show($"✓ {service.DisplayName} startup type changed to {newStartupType}.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    //this is a simple Refresh for the list
                    await LoadServices();
                }
                else
                {
                    MessageBox.Show($"❌ Failed to change {service.DisplayName} startup type.\n\n" +
                        $"Try running AnalyzeMe as Administrator.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                button.Content = originalContent;
                button.IsEnabled = true;
            }
        }

        private void ProgramSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProgramSearchBox == null || ProgramsListView == null) return;

            var searchText = ProgramSearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                ProgramsListView.ItemsSource = _allPrograms;
            }
            else
            {
                var filtered = _allPrograms.Where(p =>
                    p.Name?.ToLower().Contains(searchText) == true ||
                    p.Publisher?.ToLower().Contains(searchText) == true).ToList();

                ProgramsListView.ItemsSource = filtered;
            }
        }

        private void ServiceSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ServiceSearchBox == null || ServicesListView == null) return;

            var searchText = ServiceSearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                ServicesListView.ItemsSource = _allServices;
            }
            else
            {
                var filtered = _allServices.Where(s =>
                    s.Name?.ToLower().Contains(searchText) == true ||
                    s.DisplayName?.ToLower().Contains(searchText) == true ||
                    s.Description?.ToLower().Contains(searchText) == true).ToList();

                ServicesListView.ItemsSource = filtered;
            }
        }
    }
}