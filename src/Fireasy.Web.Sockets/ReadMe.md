Fireasy.Web.Sockets 在 .Net Core 下实现了类似于 SignalR 的集线器功能。最新版本使用 redis 缓存及消息队列实现了群集部署。

<b>Startup 配置</b>

```C#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseWebSockets(options =>
    {
        options.HeartbeatInterval = TimeSpan.FromSeconds(10);  //心跳检测时间间隔
        options.ReceiveBufferSize = 4 * 1024;
        options.KeepAliveInterval = TimeSpan.FromSeconds(10);
        options.Distributed = true; //集群支持
        options.MapHandler<NotifyHandler>("/wsNotify");
    });
}
```

<b>Handler示例</b>

```C#
public class NotifyHandler : WebSocketHandler
{
    private static SessionManager<int> manager = new SessionManager<int>();

    public void Connect(int userId)
    {
        manager.Add(ConnectionId, userId);
    }

    /// <summary>
    /// 发送消息。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="sender"></param>
    /// <param name="receiver"></param>
    /// <param name="type"></param>
    /// <param name="message"></param>
    public void Send(string key, int sender, int receiver, int type, string message)
    {
        var receiveConnId = manager.FindConnection(receiver);

        if (!string.IsNullOrEmpty(receiveConnId))
        {
            //接收者消息通知
            Clients.Client(receiveConnId).SendAsync("onReceive", sender, message);
        }
    }
}
```

<b>客户端调用</b>

```C#
public async Task SendAsync()
{
    var client = new WebSocketClient();
    await client.StartAsync("http://localhost/wsNotify");
    await client.SendAsync("Send", string.Empty, 1, 2, 0, "发送消息给你了");
    await client.CloseAsync();
}
```
