==== .net framework 配置 ====

<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <sectionGroup name="fireasy">
      <section name="cachings" type="Fireasy.Common.Caching.Configuration.CachingConfigurationSectionHandler, Fireasy.Common" />
    </sectionGroup>
  </configSections>
  <fireasy>
    <cachings>
      <caching type="Fireasy.Redis.CacheManager, Fireasy.Redis">
        <config defaultDb="2">
          <host server="localhost"></host>
        </config>
      </caching>
    </cachings>
  </fireasy>
</configuration>

==== .net core 配置 ====

{
  "fireasy": {
    "cachings": {
      "settings": {
        "redis": {
          "type": "Fireasy.Redis.CacheManager, Fireasy.Redis",
          "config": {
            "defaultDb": 1,
            "host": [
              {
                "server": "localhost"
              }
            ]
          }
        }
      }
	}
  }
}
