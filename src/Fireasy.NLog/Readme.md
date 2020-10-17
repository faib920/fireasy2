==== .net framework 配置 ====
```XML
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <sectionGroup name="fireasy">
      <section name="loggings" type="Fireasy.Common.Logging.Configuration.LoggingConfigurationSectionHandler, Fireasy.Common"/>
    </sectionGroup>
  </configSections>
  <fireasy>
    <loggings managed="Fireasy.NLog.LogFactory,Fireasy.NLog">
    </loggings>
  </fireasy>
</configuration>
```

==== .net core 配置 ====
```json
{
  "fireasy": {
    "loggings": {
      "settings": {
        "nlog": {
          "type": "Fireasy.NLog.LogFactory,Fireasy.NLog"
        }
      }
    }
  }
}
```
