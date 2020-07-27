该程序集提供对 阿里云 RabbitMQ 的支持。

Nuget 里搜索 Fireasy.Aliyun.AMQP 或 通过程序包管理器控制台输入命令进行安装： 

Install-Package Fireasy.Aliyun.AMQP

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
      <subscriber type="Fireasy.Aliyun.AMQP.SubscribeManager, Fireasy.Aliyun.AMQP">
        <config userName="your accessKey Id" password="your accessKey Secret" server="amqp://11398204--07994.mq-amqp.cn-beijing-327844-a.aliyuncs.com" virtualHost="test" />
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
        "amqp": {
          "type": "Fireasy.Aliyun.AMQP.SubscribeManager, Fireasy.Aliyun.AMQP",
          "config": {
            "userName": "your accessKey Id",
            "password": "your accessKey Secret",
            "server": "amqp://11398204--07994.mq-amqp.cn-beijing-327844-a.aliyuncs.com",
            "virtualHost": "test",
            "requeueDelayTime": 2000
          }
        }
      }
    }
  }
}
```
