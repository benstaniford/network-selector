using Microsoft.Win32;

namespace NetworkSwitcher;

internal sealed class Settings
{
    public const string DefaultSsid = "vodafoneC72225";
    internal const string DefaultRegistryPath = @"Software\NetworkSwitcher";
    private const string SsidValueName = "DesiredSsid";
    private const string DisableEthernetValueName = "DisableEthernet";

    public string DesiredSsid { get; set; } = DefaultSsid;
    public bool DisableEthernet { get; set; }

    public static Settings Load() => Load(DefaultRegistryPath);

    public void Save() => Save(DefaultRegistryPath);

    internal static Settings Load(string registryPath)
    {
        var settings = new Settings();
        using var key = Registry.CurrentUser.OpenSubKey(registryPath);
        if (key is null)
            return settings;
        if (key.GetValue(SsidValueName) is string ssid && !string.IsNullOrWhiteSpace(ssid))
            settings.DesiredSsid = ssid;
        if (key.GetValue(DisableEthernetValueName) is int flag)
            settings.DisableEthernet = flag != 0;
        return settings;
    }

    internal void Save(string registryPath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(registryPath)
            ?? throw new InvalidOperationException($"Unable to open registry key {registryPath}.");
        key.SetValue(SsidValueName, string.IsNullOrWhiteSpace(DesiredSsid) ? DefaultSsid : DesiredSsid, RegistryValueKind.String);
        key.SetValue(DisableEthernetValueName, DisableEthernet ? 1 : 0, RegistryValueKind.DWord);
    }
}
