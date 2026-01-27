using System.IO;
using System.Text;
using HyperVProxyManager.Models;
using HyperVProxyManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HyperVProxyManager.Services;

public class SettingsService : ISettingsService
{
    private const string CONFIG_FILE_NAME = "config.ini";

    public AppConfig Config { get; } = new();

    public void Load()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, CONFIG_FILE_NAME);

        if (!File.Exists(filePath))
            return; // 文件不存在，保持内存中的默认值

        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddIniFile(CONFIG_FILE_NAME, optional: true, reloadOnChange: false);

            var root = builder.Build();

            // 读取 UseExternalConfig
            string? useExtVal = root["use_external_config"];
            bool useExt = useExtVal == "1" || (bool.TryParse(useExtVal, out bool b) && b);

            Config.UseExternalConfig = useExt;

            if (useExt)
            {
                // 仅当启用外部配置时，才尝试读取端口
                string? portVal = root["proxy_port"];
                if (int.TryParse(portVal, out int port) && IsValidPort(port))
                {
                    Config.ProxyPort = port;
                    Config.UseExternalConfig = true;
                }
                else
                {
                    Config.ProxyPort = AppConfig.DEFAULT_PORT;
                    Config.UseExternalConfig = false;
                }
            }
            else
            {
                Config.ProxyPort = AppConfig.DEFAULT_PORT;
                Config.UseExternalConfig = false;
            }
        }
        catch
        { /* 读取出错时忽略，使用默认值 */ }
    }

    public void Save()
    {
        int portToWrite = Config.UseExternalConfig
            ? Config.ProxyPort
            : AppConfig.DEFAULT_PORT;

        string filePath = Path.Combine(AppContext.BaseDirectory, CONFIG_FILE_NAME);
        var sb = new StringBuilder();
        _ = sb
            .AppendLine($"use_external_config={(Config.UseExternalConfig ? 1 : 0)}")
            .AppendLine($"proxy_port={portToWrite}");

        try
        {
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
        catch
        { /* 忽略写入错误 */ }
    }

    private static bool IsValidPort(int port)
        => port is >= 1 and <= 65535;
}
