using System.Reflection;
using System.Windows;

namespace NetworkSwitcher;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by WPF XAML framework")]
internal sealed partial class MainWindow : Window, IDisposable
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
                $"Network Switcher v{version}\n\nMonitors WiFi and keeps you connected to vodafoneC72225.",
                "About Network Switcher",
                MessageBoxButton.OK,
                MessageBoxImage.Information));
    }

    public void Dispose() => _monitor.Dispose();

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
