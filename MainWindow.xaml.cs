using System.Reflection;
using System.Windows;

namespace SampleTrayApp;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by WPF XAML framework")]
internal sealed partial class MainWindow : Window
{
    private readonly NetworkMonitor _monitor = new();

    public MainWindow()
    {
        InitializeComponent();
        _monitor.StatusChanged += OnStatusChanged;
        _monitor.Start();
    }

    private void OnStatusChanged(object? sender, string status)
    {
        Dispatcher.BeginInvoke(() =>
        {
            StatusMenuItem.Header = status;
            TrayIcon.ToolTipText = status;
        });
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "dev";
        Dispatcher.BeginInvoke(() =>
            MessageBox.Show(
                $"Network Selector v{version}\n\nMonitors WiFi and keeps you connected to vodafoneC72225.",
                "About Network Selector",
                MessageBoxButton.OK,
                MessageBoxImage.Information));
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _monitor.Dispose();
        Application.Current.Shutdown();
    }

    private void TrayIcon_LeftClick(object sender, RoutedEventArgs e)
    {
        if (TrayIcon?.ContextMenu != null)
        {
            TrayIcon.ContextMenu.IsOpen = true;
        }
    }
}
