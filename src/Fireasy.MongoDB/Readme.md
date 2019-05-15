该程序集提供对 MongoDB 的支持，即在不改动任何代码的情况下，就可以切换到 MongoDB 数据库。

Nuget 里搜索 Fireasy.MongoDB 或 通过程序包管理器控制台输入命令进行安装： 

Install-Package Fireasy.MongoDB

==== .net framework 配置 ====
```XML
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <sectionGroup name="fireasy">
      <section name="dataInstances" type="Fireasy.Data.Configuration.InstanceConfigurationSectionHandler, Fireasy.Data" />
      <section name="dataProviders" type="Fireasy.Data.Provider.Configuration.ProviderConfigurationSectionHandler, Fireasy.Data"/>
    </sectionGroup>
  </configSections>
  <fireasy>
    <dataInstances>
      <instance providerName="mongodb" connectionString="server=mongodb://localhost;database=test"></instance>
    </dataInstances>
    <dataProviders>
      <provider name="mongodb" type="Fireasy.MongoDB.MongoDBProvider, Fireasy.MongoDB"></provider>
    </dataProviders>
  </fireasy>
</configuration>
```

==== .net core 配置 ====
```json
{
  "fireasy": {
    "dataInstances": {
      "settings": {
        "mongodb": {
          "providerName": "mongodb",
          "connectionString": "server=mongodb://localhost;database=test"
        }
      }
    },
    "dataProviders": {
      "settings": {
        "mongodb": {
          "type": "Fireasy.MongoDB.MongoDBProvider, Fireasy.MongoDB"
        }
      }
    }
  }
}
```
