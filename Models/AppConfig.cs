using CommunityToolkit.Mvvm.ComponentModel;

namespace HyperVProxyManager.Models;

public partial class AppConfig : ObservableObject
{
    public const int DEFAULT_PORT = 7890;

    [ObservableProperty]
    public partial bool UseExternalConfig { get; set; } = false;

    [ObservableProperty]
    public partial int ProxyPort { get; set; } = DEFAULT_PORT;

    partial void OnUseExternalConfigChanged(bool value)
    {
        if (!value)
            ProxyPort = DEFAULT_PORT;
    }
}
