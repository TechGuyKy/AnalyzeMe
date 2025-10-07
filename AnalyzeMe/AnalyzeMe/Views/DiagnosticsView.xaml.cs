using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AnalyzeMe.Models;
using AnalyzeMe.ViewModels;

namespace AnalyzeMe.Views
{
    public partial class DiagnosticsView : Page
    {
        private MainViewModel ViewModel => (MainViewModel)Application.Current.MainWindow.DataContext;

        public DiagnosticsView()
        {
            InitializeComponent();
            DataContext = ViewModel;
            Loaded += DiagnosticsView_Loaded;
        }

        private void DiagnosticsView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            DiagnosticsListView.ItemsSource = ViewModel.DiagnosticResults;
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            var results = ViewModel.DiagnosticResults;

            CriticalCountText.Text = results.Count(r => r.Severity == DiagnosticSeverity.Critical).ToString();
            ErrorCountText.Text = results.Count(r => r.Severity == DiagnosticSeverity.Error).ToString();
            WarningCountText.Text = results.Count(r => r.Severity == DiagnosticSeverity.Warning).ToString();
            InfoCountText.Text = results.Count(r => r.Severity == DiagnosticSeverity.Info).ToString();
        }

        private async void RunScanButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
                button.Content = "⏳ SCANNING...";
            }

            await ViewModel.RunDiagnosticsAsync();
            UpdateDisplay();

            if (button != null)
            {
                button.IsEnabled = true;
                button.Content = "🔄 RUN FULL SCAN";
            }

            var criticalCount = ViewModel.DiagnosticResults.Count(r => r.Severity == DiagnosticSeverity.Critical);
            var errorCount = ViewModel.DiagnosticResults.Count(r => r.Severity == DiagnosticSeverity.Error);
            var warningCount = ViewModel.DiagnosticResults.Count(r => r.Severity == DiagnosticSeverity.Warning);

            string message = $"Diagnostic scan complete!\n\n" +
                           $"Found:\n" +
                           $"• {criticalCount} Critical issue(s)\n" +
                           $"• {errorCount} Error(s)\n" +
                           $"• {warningCount} Warning(s)\n" +
                           $"• {ViewModel.DiagnosticResults.Count} Total result(s)";

            MessageBox.Show(message, "Scan Complete", MessageBoxButton.OK,
                criticalCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
    }
}