using Microsoft.Win32;
using NetworkSwitcher;
using Xunit;

namespace NetworkSwitcher.Tests;

public sealed class SettingsTests : IDisposable
{
    private readonly string _testRegistryPath = $@"Software\NetworkSwitcher.Tests\{Guid.NewGuid():N}";

    [Fact]
    public void Load_WhenKeyMissing_ReturnsDefaults()
    {
        var settings = Settings.Load(_testRegistryPath);

        Assert.Equal(Settings.DefaultSsid, settings.DesiredSsid);
        Assert.False(settings.DisableEthernet);
    }

    [Fact]
    public void Save_ThenLoad_RoundTripsValues()
    {
        var saved = new Settings
        {
            DesiredSsid = "MyHomeWifi",
            DisableEthernet = true,
        };
        saved.Save(_testRegistryPath);

        var loaded = Settings.Load(_testRegistryPath);

        Assert.Equal("MyHomeWifi", loaded.DesiredSsid);
        Assert.True(loaded.DisableEthernet);
    }

    [Fact]
    public void Save_BlankSsid_PersistsDefaultSsid()
    {
        var saved = new Settings
        {
            DesiredSsid = "   ",
            DisableEthernet = false,
        };
        saved.Save(_testRegistryPath);

        var loaded = Settings.Load(_testRegistryPath);

        Assert.Equal(Settings.DefaultSsid, loaded.DesiredSsid);
    }

    public void Dispose()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(_testRegistryPath, throwOnMissingSubKey: false);
            Registry.CurrentUser.DeleteSubKey(@"Software\NetworkSwitcher.Tests", throwOnMissingSubKey: false);
        }
        catch (IOException)
        {
            // Other parallel tests may still hold the parent key — ignore.
        }
    }
}
