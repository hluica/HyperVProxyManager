namespace HyperVProxyManager.Models;

// 定义代理状态的简单数据模型
public record ProxyState
{
    public bool IsEnabled { get; init; }
    public string ServerAddress { get; init; } = string.Empty;

    public static ProxyState Empty
        => new() { IsEnabled = false, ServerAddress = "未设置" };
}
