using System.Windows;

namespace NetworkSwitcher;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by WPF XAML framework")]
internal sealed partial class PreferencesWindow : Window
{
    public Settings Settings { get; }

    public PreferencesWindow(Settings settings)
    {
        InitializeComponent();
        Settings = settings;
        SsidTextBox.Text = settings.DesiredSsid;
        DisableEthernetCheckBox.IsChecked = settings.DisableEthernet;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var ssid = SsidTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(ssid))
        {
            MessageBox.Show(
                "SSID cannot be empty.",
                "Invalid Setting",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        Settings.DesiredSsid = ssid;
        Settings.DisableEthernet = DisableEthernetCheckBox.IsChecked == true;
        Settings.Save();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
