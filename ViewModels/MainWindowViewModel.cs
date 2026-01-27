using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HyperVProxyManager.Core;
using Microsoft.Extensions.DependencyInjection;

namespace HyperVProxyManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IRecipient<NavigationMessage>
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private bool _isSettingsButtonVisible = true;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        WeakReferenceMessenger.Default.Register(this);

        // 默认显示仪表盘
        NavigateToDashboard();
    }

    public void Receive(NavigationMessage message)
    {
        if (message.Value == "Dashboard")
            NavigateToDashboard();
        else if (message.Value == "Settings")
            NavigateToSettings();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentView = _serviceProvider.GetRequiredService<SettingsViewModel>();
        IsSettingsButtonVisible = false; // 进入设置页隐藏设置按钮
    }

    private void NavigateToDashboard()
    {
        CurrentView = _serviceProvider.GetRequiredService<DashboardViewModel>();
        IsSettingsButtonVisible = true; // 回到主页显示设置按钮
    }
}
