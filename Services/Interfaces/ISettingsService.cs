using HyperVProxyManager.Models;

namespace HyperVProxyManager.Services.Interfaces;

public interface ISettingsService
{
    AppConfig Config { get; }
    void Load();
    void Save();
}
