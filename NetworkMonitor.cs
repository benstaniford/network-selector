using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace NetworkSwitcher;

internal sealed partial class NetworkMonitor : IDisposable
{
    private const string TargetSsid = "vodafoneC72225";
    private const int PreferredWifiMetric = 1;
    private static readonly TimeSpan SwitchCooldown = TimeSpan.FromSeconds(5);

    private readonly Timer _pollTimer;
    private DateTime _lastSwitchAttempt = DateTime.MinValue;
    private string? _wifiInterfaceName;
    private bool _metricLowered;
    private bool _disposed;

    public event EventHandler<string>? StatusChanged;

    public NetworkMonitor()
    {
        _pollTimer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
        Task.Run(EnsureConnectedToTarget);
        // Poll every minute as backup in case events are missed
        _pollTimer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    private void OnNetworkAddressChanged(object? sender, EventArgs e) =>
        Task.Run(EnsureConnectedToTarget);

    private void OnTimerElapsed(object? state) =>
        EnsureConnectedToTarget();

    private void EnsureConnectedToTarget()
    {
        var current = GetCurrentSsid();

        if (current == TargetSsid)
        {
            ApplyWifiPreference();
            StatusChanged?.Invoke(this, $"Connected to {TargetSsid}");
            return;
        }

        RestoreWifiMetric();

        // Debounce: don't hammer netsh if we just tried
        if (DateTime.Now - _lastSwitchAttempt < SwitchCooldown)
            return;

        _lastSwitchAttempt = DateTime.Now;
        var from = current is null ? "no WiFi" : $"\"{current}\"";
        StatusChanged?.Invoke(this, $"Switching from {from} to {TargetSsid}...");
        ConnectToTarget();

        // Brief wait then re-check to emit updated status
        Thread.Sleep(3000);
        var after = GetCurrentSsid();
        if (after == TargetSsid)
        {
            ApplyWifiPreference();
            StatusChanged?.Invoke(this, $"Connected to {TargetSsid}");
        }
        else
        {
            StatusChanged?.Invoke(this, $"Failed to connect to {TargetSsid}");
        }
    }

    private void ApplyWifiPreference()
    {
        if (_metricLowered)
            return;
        _wifiInterfaceName ??= GetWifiInterfaceName();
        if (string.IsNullOrEmpty(_wifiInterfaceName))
            return;
        RunNetsh($"interface ipv4 set interface \"{_wifiInterfaceName}\" metric={PreferredWifiMetric}");
        RunNetsh($"interface ipv6 set interface \"{_wifiInterfaceName}\" metric={PreferredWifiMetric}");
        _metricLowered = true;
    }

    private void RestoreWifiMetric()
    {
        if (!_metricLowered || string.IsNullOrEmpty(_wifiInterfaceName))
            return;
        RunProcess("powershell",
            $"-NoProfile -NonInteractive -Command \"Set-NetIPInterface -InterfaceAlias '{_wifiInterfaceName}' -AutomaticMetric Enabled\"");
        _metricLowered = false;
    }

    private static string? GetWifiInterfaceName()
    {
        var output = RunNetsh("wlan show interfaces");
        var match = NameRegex().Match(output);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? GetCurrentSsid()
    {
        var output = RunNetsh("wlan show interfaces");
        var match = SsidRegex().Match(output);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static void ConnectToTarget() =>
        RunNetsh($"wlan connect name=\"{TargetSsid}\"");

    private static string RunNetsh(string args) => RunProcess("netsh", args);

    private static string RunProcess(string fileName, string args)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    [GeneratedRegex(@"^\s+SSID\s+:\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex SsidRegex();

    [GeneratedRegex(@"^\s+Name\s+:\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex NameRegex();

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
        _pollTimer.Dispose();
        RestoreWifiMetric();
    }
}
