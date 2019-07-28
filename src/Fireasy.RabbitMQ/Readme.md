该程序集提供 RibbitMQ 消息订阅的实现。

实现了 ISubscribeManager 接口，用时只需使用 SubscribeManagerFactory.CreateManager() 即可。

Nuget 里搜索 Fireasy.RabbitMQ 或 通过程序包管理器控制台输入命令进行安装： 

Install-Package Fireasy.RabbitMQ

```C#
//主题的定义
public class TestSubject
{
    public string Message { get; set; }
}

//发布消息
public void TestPublish()
{
    var sub = SubscribeManagerFactory.CreateManager();
    sub.Publish(new TestSubject { Message = "fireasy" });
}

//订阅消息
public void TestSubscribe()
{
    var sub = SubscribeManagerFactory.CreateManager();
    sub.AddSubscriber<TestSubject>(subject => Console.WriteLine(subject.Message));
}
```

新增 exchangeType 配置，如果未指定此值，消息平均分配给不同的消费者，如果指定为 topic，则所有消费者均同时接收到消息。

==== .net framework 配置 ====
```XML
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <sectionGroup name="fireasy">
      <section name="subscribers" type="Fireasy.Common.Subscribes.Configuration.SubscribeConfigurationSectionHandler, Fireasy.Common" />
    </sectionGroup>
  </configSections>
  <fireasy>
    <subscribers>
      <subscriber type="Fireasy.RabbitMQ.SubscribeManager, Fireasy.RabbitMQ">
        <config userName="guest" password="123" server="amqp://127.0.0.1:5672" />
      </subscriber>
    </subscribers>
  </fireasy>
</configuration>
```

==== .net core 配置 ====
```json
{
  "fireasy": {
    "subscribers": {
      "settings": {
        "redis": {
          "type": "Fireasy.RabbitMQ.SubscribeManager, Fireasy.RabbitMQ",
          "config": {
             "userName": "guest",
             "password": "123",
             "server": "amqp://127.0.0.1:5672",
             "exchangeType": "topic"
          }
        }
      }
    }
  }
}
```
