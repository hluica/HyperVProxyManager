using CommunityToolkit.Mvvm.Messaging.Messages;

namespace HyperVProxyManager.Utils;

// 用于在 ViewModel 之间传递页面导航请求
public class NavigationMessage(string targetPage)
    : ValueChangedMessage<string>(targetPage)
{ }
