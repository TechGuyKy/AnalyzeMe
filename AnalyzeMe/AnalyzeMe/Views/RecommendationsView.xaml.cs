using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AnalyzeMe.Models;
using AnalyzeMe.ViewModels;

namespace AnalyzeMe.Views
{
    public partial class RecommendationsView : Page
    {
        private MainViewModel ViewModel => (MainViewModel)Application.Current.MainWindow.DataContext;

        public RecommendationsView()
        {
            InitializeComponent();
            DataContext = ViewModel;
            Loaded += RecommendationsView_Loaded;
        }

        private void RecommendationsView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            RecommendationsListView.ItemsSource = ViewModel.Recommendations;
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            var recommendations = ViewModel.Recommendations;

            CriticalCountText.Text = recommendations.Count(r => r.Priority == RecommendationPriority.Critical).ToString();
            HighCountText.Text = recommendations.Count(r => r.Priority == RecommendationPriority.High).ToString();
            MediumCountText.Text = recommendations.Count(r => r.Priority == RecommendationPriority.Medium).ToString();
            LowCountText.Text = recommendations.Count(r => r.Priority == RecommendationPriority.Low).ToString();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
                button.Content = "⏳ GENERATING...";
            }

            await ViewModel.GenerateRecommendationsAsync();
            UpdateDisplay();

            if (button != null)
            {
                button.IsEnabled = true;
                button.Content = "🔄 REFRESH";
            }

            var criticalCount = ViewModel.Recommendations.Count(r => r.Priority == RecommendationPriority.Critical);
            var highCount = ViewModel.Recommendations.Count(r => r.Priority == RecommendationPriority.High);

            string message = $"Recommendations generated!\n\n" +
                           $"Found:\n" +
                           $"• {criticalCount} Critical priority\n" +
                           $"• {highCount} High priority\n" +
                           $"• {ViewModel.Recommendations.Count} Total recommendation(s)\n\n" +
                           $"Review the suggestions below to optimize your system.";

            MessageBox.Show(message, "Recommendations Ready", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}