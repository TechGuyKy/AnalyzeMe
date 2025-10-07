using System.Windows;

namespace AnalyzeMe
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Resources.Add("EqualityConverter", new Converters.EqualityConverter());
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"An error occurred: {args.Exception.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}