using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using HyperVProxyManager.Services.Interfaces;
using HyperVProxyManager.Utils;

namespace HyperVProxyManager.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _settingsService.Config.PropertyChanged += (_, e)
            => OnPropertyChanged(e.PropertyName);
    }

    public bool UseExternalConfig
    {
        get => _settingsService.Config.UseExternalConfig;
        set => _settingsService.Config.UseExternalConfig = value;
    }

    public int ProxyPort
    {
        get => _settingsService.Config.ProxyPort;
        set
        {
            if (UseExternalConfig && value is > 0 and <= 65535)
                _settingsService.Config.ProxyPort = value;
        }
    }

    [RelayCommand]
    private static void GoBack()
        => _ = WeakReferenceMessenger.Default.Send(new NavigationMessage("Dashboard"));
}
