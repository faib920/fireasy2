该程序集提供 Redis 缓存及消息订阅两个实现。

一、缓存
    实现了 ICacheManager 接口，用时只需使用 CacheManagerFactory.CreateManager() 即可。

```C#
public void TestTryGet()
{
    var cacheMgr = CacheManagerFactory.CreateManager();
    var value = cacheMgr.TryGet("test1", () => 100);
    Assert.AreEqual(100, value);
}

public void TestContains()
{
    var cacheMgr = CacheManagerFactory.CreateManager();
    var value = cacheMgr.TryGet("test1", () => 100);
    Assert.AreEqual(true, cacheMgr.Contains("test1"));
    Assert.AreEqual(false, cacheMgr.Contains("test2"));
}

public void TestExpired()
{
    var cacheMgr = CacheManagerFactory.CreateManager();
    var value = cacheMgr.TryGet("test3", () => 100, () => new RelativeTime(TimeSpan.FromSeconds(2)));
    Assert.AreEqual(true, cacheMgr.Contains("test3"));
    Thread.Sleep(3000);
    Assert.AreEqual(false, cacheMgr.Contains("test3"));
}

public void TestClear()
{
    var cacheMgr = CacheManagerFactory.CreateManager();
    cacheMgr.Add("test4", 100);
    cacheMgr.Add("test5", 100);
    cacheMgr.Add("test6", 100);

    cacheMgr.Clear();

    Assert.AreEqual(false, cacheMgr.Contains("test4"));
    Assert.AreEqual(false, cacheMgr.Contains("test5"));
    Assert.AreEqual(false, cacheMgr.Contains("test6"));
}
```

==== .net framework 配置 ====
```XML
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
```

==== .net core 配置 ====
```json
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
```

二、消息订阅
    实现了 ISubscribeManager 接口，用时只需使用 SubscribeManagerFactory.CreateManager() 即可。

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
      <subscriber type="Fireasy.Redis.RedisSubscribeManager, Fireasy.Redis">
        <config defaultDb="2">
          <host server="localhost"></host>
        </config>
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
          "type": "Fireasy.Redis.RedisSubscribeManager, Fireasy.Redis",
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
```
