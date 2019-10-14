# fireasy2

Fireasy是一套基于.Net Framework应用开发组件，其主旨思想为“让开发变为更简单”，其义为，使用尽可能少的组件，实现你所需的功能。Fireasy几乎覆盖了开发中可能使用到的技术，比如Log、Cache、AOP、IOC、ORM、MVC等等，并提供了Redis、RabbitMQ、NLog、Log4net等这些组件的适配。

Fireasy 支持 net3.5(已废弃)/net4.0(已废弃)/net4.5/net4.6/net4.7/netstandard2.0/netstandard2.1，只需一个dll即可，已发布到 nuget ，搜索添加即可。

*	[Fireasy.Common](https://www.nuget.org/packages/Fireasy.Common)
*	[Fireasy.Data](https://www.nuget.org/packages/Fireasy.Data)
*	[Fireasy.Data.Entity](https://www.nuget.org/packages/Fireasy.Data.Entity)
*	[Fireasy.Web.Mvc](https://www.nuget.org/packages/Fireasy.Web.Mvc)
*	[Fireasy.Web.EasyUI](https://www.nuget.org/packages/Fireasy.Web.EasyUI)
*	[Fireasy.Redis](https://www.nuget.org/packages/Fireasy.Redis)
*	[Fireasy.RabbitMQ](https://www.nuget.org/packages/Fireasy.RabbitMQ)
*	[Fireasy.NLog](https://www.nuget.org/packages/Fireasy.NLog)
*	[Fireasy.Log4net](https://www.nuget.org/packages/Fireasy.Log4net)

Fireasy 的Demo项目 [https://github.com/faib920/zero](https://github.com/faib920/zero)。

Fireasy 的技术博客 [http://fireasy.cnblogs.com](http://fireasy.cnblogs.com)。

Fireasy 的使用手册 [《Fireasy从入门到精通》](http://www.fireasy.cn/docs/fireasy2.pdf)。

<b>`Fireasy.Common`</b>

是一组公用的类库，提供一些常用的类和方法。
*	配置处理：基于System.Configuration扩展，提供自定义配置节的定义、解析和扩展。尤其可以将基于Fireasy的配置节定义到一个外部配置文件中。
*	缓存管理：提供应用程序缓存管理接口。
*	日志管理：提供应用程序日志模型及管理接口。
*	代码编译：基于CodeDom扩展，将一段代码编译为一个类，或是一个方法委托。
*	动态编译：基于Emit扩展，简化了动态编织代码的过程。
*	密码安全：提供加密解密接口， DES、MD5、RC2、SHA1等实现。
*	对象序列化：提供对象序列化反序列化接口，二进制加密、压缩序列化、Json序列化的实现。
*	控制反转：一个简单实用的IOC。
*	面向方面：一个简单实用的AOP。
*	MEF扩展：提供对MEF的配置、过滤。
*	扩展方法：提供字符串、日期、类型、中文、反射等常用的扩展方法。
*	线程共享：提供一个Scope达到线程内数据共享。
*	事件订阅：提供主题发布/事件订阅机制。

<b>`Fireasy.Data`</b>

数据库底层的访问类库，目前支持MsSql、Oracle、MySql、SQLite、PostgreSql、Firebird等常见数据库。
*	数据库操作：提供执行命令、填充DataSet、返回Enumerable、返回DataReader、Update更新、事务操作等方法。
*	命令追踪：在连接字符串里配置tracking即可以追踪数据访问对象所执行的每一条命令，以及执行命令耗用的时间。
*	实例配置：提供配置文件、xml、binary、注册表等多种配置。
*	提供者扩展：对MsSql、Oracle等提供者进行插件式扩展。包括：数据备份扩展、数据批量插入扩展、数据记录包装扩展、数据架构扩展、数据语法扩展和生成器扩展。
*	富类型转换：可以将Color、Image、Font、Point、Exception等对象放到库中，并且从库中读取。
*	分页评估：提供大数据量和小数据量场景下的分页计算方法。

```C#
public void Sample()
{
    using (var db = DatabaseFactory.CreateDatabase())
    {
        //查询返回
        var customers = db.ExecuteEnumerable<Customer>((SqlCommand)"select * from customers");
		
        //分页，参数化
        var ds = new DataSet();
        var paper = new DataPager(5, 0);
        var parameters = new ParameterCollection();
        parameters.Add("city", "London");
        db.FillDataSet(ds, (SqlCommand)"select city from customers where city <> @city", segment: paper, parameters: parameters);
		
        //批量插入
        var list = new List<BatcherData>();

        for (var i = 0; i < 100000; i++)
        {
            list.Add(new BatcherData { ID = i, NAME = new Size(12, 20), BIRTHDAY = DateTime.Now });
        }

        var provider = db.Provider.GetService<IBatcherProvider>();
        provider.Insert(db, list, "BATCHERS");
		
        //获取所有表定义
        var schema = db.Provider.GetService<ISchemaProvider>();
        var parameter = db.Provider.GetConnectionParameter(db.ConnectionString);
        foreach (var table in schema.GetSchemas<Table>(db, s => s.Schema == parameter.Schema))
        {
            Console.WriteLine(table.Name + "," + table.Description);
        }
    }
}
```

<b>`Fireasy.Data.Entity`</b>

实体框架，Linq解析部份参考了iqtoolkit和NLite开源框架。
*	依赖属性映射：采用WPF中依赖属性的方法进行字段属性的映射。
*	LINQ查询：支持常用的LINQ查询。
*	实体关系：与Entity Framework类似，可以定义实体间的关系，方便LINQ的关联查询。
*	逻辑删除标记：实体设置逻辑删除标记后，查询中将过滤这些已经被标记的数据。
*	延迟加载：对于关联属性，可以在需要的时候才从库中读取加载。
*	枚举描述属性：支持枚举属性，同时还可以定义与枚举相关联的文本描述作为附加的属性。
*	子查询属性：可以定义一个子查询的属性。
*	树结构：采用类似00010001的编码来管理树结构，提供插入、移动、枚举孩子、递归父亲、获取兄弟等方法。
*	数据验证：基于DataAnnotations制定实体的数据验证规则。
*	持久化事务：基于Scope定义线程内的事务控制。
*	持久化环境：根据环境内的参数，格式化实体所映射的表名称，实现数据表横向扩展。
*	动态持久化：通过动态构造实体类型，实现其持久化操作。
*	实体上下文：提供类似于Entity Framework的数据上下文。
*	惰性加载：在枚举实体序列并使用关联属性时，由于延迟加载机制将发生n+1次数据库查询动作，此时可以使用Include方法将关联属性预先加载出来。
*	数据缓存：提供LINQ解析缓存和数据缓存。

```C#
public void Sample()
{
    using (var context = new DbContext())
    {
        DateTime? startTime = null;
        DateTime? endTime = null;
        var state = 0;

        //AssertWhere 用法
        var orders = context.Orders
            .AssertWhere(startTime != null, s => s.OrderDate >= startTime)
            .AssertWhere(endTime != null, s => s.OrderDate <= endTime)
            .AssertWhere(state == 0, s => s.RequiredDate == DateTime.Now, s => s.RequiredDate >= DateTime.Now)
			.AsNoTracking();
	
        //ExtandAs 扩展用法
        var details = context.OrderDetails.Select(s =>
            s.ExtendAs<OrderDetails>(() => new OrderDetails
                {
                    ProductName = s.Products.ProductName
                }))
            .ToList();
		
        //分页
        var pager = new DataPager(50, 2);
        var products = context.Products.Segment(pager).ToList();
		
        //排序
        var sorting = new SortDefinition();
        sorting.Member = "OrderDate";
        sorting.Order = SortOrder.Descending;

        var orders1 = context.Orders
            .Select(s => new { s.OrderDate, CompanyName = s.Customers.CompanyName })
            .OrderBy(sorting, u => u.OrderByDescending(s => s.OrderDate))
            .ToList();
		
        //按条件更新
        context.Orders.Update(() => new Orders { Freight = 1 }, s => s.OrderDate >= DateTime.Now);
		
        //计算器方式更新
        context.Orders.Update(s => new Orders { Freight = s.Freight * 100 }, s => s.OrderDate >= DateTime.Now);
		
        //按条件删除
        context.Orders.Delete(s => s.OrderDate > DateTime.Now);

        //Batch插入
        var depts = new List<Depts>();

        for (var i = 0; i < 100; i++)
        {
            var d = Depts.New();
            d.DeptName = "测试" + i;
            depts.Add(d);
        }

        context.Depts.Batch(depts, (u, s) => u.Insert(s));
    }
}
```

<b>`Fireasy.Web.Mvc`</b>

针对Asp.Net MVC的扩展，提供控制器工厂，使之与IOC无缝结合。
*	Bundle配置：与MVC提供的Bundle不同的是，资源在web.config里配置。
*	控制器工厂：与IOC无缝结合，同时对Action的复杂参数进行解析，以及Json序列化转换器。
*	HTML扩展：提供常用的HTML扩展。
*	JSON包装：使用Json转换器，轻松得到想要的结果。

<b>`Fireasy.Web.EasyUI`</b>

* 为EasyUI的HTML扩展。

------------------------------------------------------------------------
QQ号： 55570729
QQ群： 6406277
------------------------------------------------------------------------

![](http://www.fireasy.cn/content/images/Donate_fireasy.png)

