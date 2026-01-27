using CommunityToolkit.Mvvm.ComponentModel;

namespace HyperVProxyManager.Models;

public partial class AppConfig : ObservableObject
{
    public const int DEFAULT_PORT = 7890;

    [ObservableProperty]
    private bool _useExternalConfig = false;

    [ObservableProperty]
    private int _proxyPort = DEFAULT_PORT;

    partial void OnUseExternalConfigChanged(bool value)
    {
        if (!value)
            ProxyPort = DEFAULT_PORT;
    }
}
