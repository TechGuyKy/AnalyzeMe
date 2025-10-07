using System.Windows;
using System.Windows.Input;
using AnalyzeMe.Views;
using AnalyzeMe.ViewModels;

namespace AnalyzeMe
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private bool _isInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _isInitialized = true;

            ContentFrame.Navigate(new DashboardView());

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.StatusMessage))
                {
                    StatusText.Text = _viewModel.StatusMessage;
                }
            };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NavigationButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            if (ContentFrame == null) return;

            if (sender == DashboardButton)
                ContentFrame.Navigate(new DashboardView());
            else if (sender == HardwareButton)
                ContentFrame.Navigate(new HardwareView());
            else if (sender == PerformanceButton)
                ContentFrame.Navigate(new PerformanceView());
            else if (sender == NetworkButton)
                ContentFrame.Navigate(new NetworkView());
            else if (sender == ProgramsButton)
                ContentFrame.Navigate(new ProgramsView());
            else if (sender == ManagerButton)
                ContentFrame.Navigate(new Views.ManagerView());
            else if (sender == DiagnosticsButton)
                ContentFrame.Navigate(new DiagnosticsView());
            else if (sender == RecommendationsButton)
                ContentFrame.Navigate(new RecommendationsView());
        }
    }
}