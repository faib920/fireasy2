该程序集解决使用 JSON.NET 序列化 Fireasy 实体类型时，造成延迟加载对象循环引用的问题。

```C#
public void TestSerialize()
{
    using (var context = new DbContext())
    {
        var customer = context.Customers.FirstOrDefault();

        var str = JsonConvert.SerializeObject(customer, new Fireasy.Newtonsoft.LazyObjectJsonConverter());
        Console.WriteLine(str);
    }
}
```

在 .net core 中全局配置如下

```C#
    services.AddMvc()
        .AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new Fireasy.Newtonsoft.LazyObjectJsonConverter());
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
```