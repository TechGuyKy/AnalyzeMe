using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using AnalyzeMe.Models;
using AnalyzeMe.Services;

namespace AnalyzeMe.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly SystemAnalyzer _systemAnalyzer;
        private readonly PerformanceMonitor _performanceMonitor;
        private readonly DiagnosticService _diagnosticService;
        private readonly RecommendationEngine _recommendationEngine;
        private readonly NetworkMonitor _networkMonitor;
        private readonly ProgramManager _programManager;
        private readonly DispatcherTimer _updateTimer;

        private SystemInfo? _systemInfo;
        private PerformanceMetrics? _currentMetrics;
        private NetworkMetrics? _currentNetworkMetrics;
        private bool _isAnalyzing;
        private string _statusMessage = "Initializing...";

        public SystemInfo? SystemInfo
        {
            get => _systemInfo;
            set => SetProperty(ref _systemInfo, value);
        }

        public PerformanceMetrics? CurrentMetrics
        {
            get => _currentMetrics;
            set => SetProperty(ref _currentMetrics, value);
        }

        public NetworkMetrics? CurrentNetworkMetrics
        {
            get => _currentNetworkMetrics;
            set => SetProperty(ref _currentNetworkMetrics, value);
        }

        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set => SetProperty(ref _isAnalyzing, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<DiagnosticResult> DiagnosticResults { get; }
        public ObservableCollection<Recommendation> Recommendations { get; }

        public MainViewModel()
        {
            _systemAnalyzer = new SystemAnalyzer();
            _performanceMonitor = new PerformanceMonitor();
            _diagnosticService = new DiagnosticService();
            _recommendationEngine = new RecommendationEngine();
            _networkMonitor = new NetworkMonitor();
            _programManager = new ProgramManager();

            DiagnosticResults = new ObservableCollection<DiagnosticResult>();
            Recommendations = new ObservableCollection<Recommendation>();

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _updateTimer.Tick += async (s, e) => await UpdateMetricsAsync();
            _updateTimer.Start();

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            IsAnalyzing = true;
            try
            {
                StatusMessage = "Analyzing system hardware...";
                SystemInfo = await _systemAnalyzer.GetSystemInfoAsync();

                StatusMessage = "Monitoring performance...";
                await UpdateMetricsAsync();

                StatusMessage = "Running diagnostics...";
                await RunDiagnosticsAsync();

                StatusMessage = "Generating recommendations...";
                await GenerateRecommendationsAsync();

                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        private async Task UpdateMetricsAsync()
        {
            try
            {
                CurrentMetrics = await _performanceMonitor.GetCurrentMetricsAsync();
                CurrentNetworkMetrics = await _networkMonitor.GetNetworkMetricsAsync();
            }
            catch
            {
                //Added catch to silently fail metric updates
            }
        }

        public async Task RunDiagnosticsAsync()
        {
            if (SystemInfo == null || CurrentMetrics == null) return;

            IsAnalyzing = true;
            StatusMessage = "Running diagnostics...";
            try
            {
                var results = await _diagnosticService.RunAllDiagnosticsAsync(SystemInfo, CurrentMetrics);
                DiagnosticResults.Clear();
                foreach (var result in results)
                {
                    DiagnosticResults.Add(result);
                }
                StatusMessage = $"Found {DiagnosticResults.Count} diagnostic result(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Diagnostic error: {ex.Message}";
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        public async Task GenerateRecommendationsAsync()
        {
            if (SystemInfo == null || CurrentMetrics == null) return;

            IsAnalyzing = true;
            StatusMessage = "Generating recommendations...";
            try
            {
                var recommendations = await _recommendationEngine.GenerateRecommendationsAsync(
                    SystemInfo, CurrentMetrics, DiagnosticResults);
                Recommendations.Clear();
                foreach (var recommendation in recommendations)
                {
                    Recommendations.Add(recommendation);
                }
                StatusMessage = $"Generated {Recommendations.Count} recommendation(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Recommendation error: {ex.Message}";
            }
            finally
            {
                IsAnalyzing = false;
            }
        }
    }
}