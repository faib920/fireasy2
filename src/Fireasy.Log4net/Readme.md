==== .net framework 配置 ====

<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <sectionGroup name="fireasy">
      <section name="loggings" type="Fireasy.Common.Logging.Configuration.LoggingConfigurationSectionHandler, Fireasy.Common"/>
    </sectionGroup>
  </configSections>
  <fireasy>
    <loggings managed="Fireasy.Log4net.LogFactory,Fireasy.Log4net">
    </loggings>
  </fireasy>
</configuration>

==== .net core 配置 ====

{
  "fireasy": {
    "loggings": {
      "settings": {
        "log4net": {
          "type": "Fireasy.Log4net.LogFactory,Fireasy.Log4net"
        }
      }
    }
  }
}
