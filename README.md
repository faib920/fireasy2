# fireasy2

Fireasy是一套基于.Net Framework应用开发组件，其主旨思想为“让开发变为更简单”，其义为，使用尽可能少的组件，实现你所需的功能。Fireasy几乎覆盖了开发中可能使用到的技术，比如Log、Cache、AOP、IOC、ORM、MVC等等，并提供了Redis、RabbitMQ、NLog、Log4net等这些组件的适配。

Fireasy 支持 net3.5(已废弃)/net4.0(已废弃)/net4.5/net4.6/net4.7/netstandard2.0/netstandard2.1，只需一个dll即可，已发布到 nuget ，搜索添加即可。

*	[Fireasy.Common](https://www.nuget.org/packages/Fireasy.Common)
*	[Fireasy.Data](https://www.nuget.org/packages/Fireasy.Data)
*	[Fireasy.Data.Entity](https://www.nuget.org/packages/Fireasy.Data.Entity)
*	[Fireasy.Web.Mvc](https://www.nuget.org/packages/Fireasy.Web.Mvc)
*	[Fireasy.Web.EasyUI](https://www.nuget.org/packages/Fireasy.Web.EasyUI)
*	[Fireasy.Web.Sockets](https://www.nuget.org/packages/Fireasy.Web.Sockets)
*	[Fireasy.Windows.Forms](https://www.nuget.org/packages/Fireasy.Windows.Forms)
*	[Fireasy.Redis](https://www.nuget.org/packages/Fireasy.Redis)
*	[Fireasy.RabbitMQ](https://www.nuget.org/packages/Fireasy.RabbitMQ)
*	[Fireasy.NLog](https://www.nuget.org/packages/Fireasy.NLog)
*	[Fireasy.Log4net](https://www.nuget.org/packages/Fireasy.Log4net)
*	[Fireasy.Newtonsoft](https://www.nuget.org/packages/Fireasy.Newtonsoft)
*	[Fireasy.MongoDB](https://www.nuget.org/packages/Fireasy.MongoDB)
*	[Fireasy.QuartzNet](https://www.nuget.org/packages/Fireasy.QuartzNet)
*	[Fireasy.Aliyun.AMQP](https://www.nuget.org/packages/Fireasy.Aliyun.AMQP)

Fireasy 的Demo项目 [https://github.com/faib920/zero](https://github.com/faib920/zero)。

Fireasy 的技术博客 [http://fireasy.cnblogs.com](http://fireasy.cnblogs.com)。

Fireasy 的使用手册 [《Fireasy从入门到精通》](http://www.fireasy.cn/docs/fireasy2.pdf)。

<b>`Fireasy.Common`</b>

是一组公用的类库，提供一些常用的类和方法。
*	配置处理：基于System.Configuration(.net framework) / IConfiguration(.net core)扩展，提供自定义配置节的定义、解析和扩展。尤其可以将基于Fireasy的配置节定义到一个外部配置文件中。
*	缓存管理：定义一组提供应用程序缓存的管理接口，并默认实现了基于内存的缓存管理。
*	日志管理：定义一组记录info、error、warn等等类型日志的接口，并默认实现了基于本地文件存储的日志管理，它的优点在于采用队列写操作。
*	代码编译：基于CodeDom扩展，可以将一段代码编译为一个类，或是一个方法委托。可应用于自定义公式中。
*	动态编译：基于Emit扩展，提供程序集、类、方法、属性、枚举等构造器，简化了动态编织代码的过程。
*	密码安全：提供加密解密接口，集成了DES、MD5、RC2、SHA1、RSA、DSA等的实现。
*	对象序列化：提供对象序列化/反序列化接口，二进制序列化可进行二进制加密、压缩处理，实现了Json和XML的序列化/反序列化，Json序列化与newtonsoft有些类似。
*	控制反转：一个简单实用的IOC，其配置文件可与.net core结合使用。
*	面向方面：一个简单实用的AOP，可对方法、属性进行拦截器注入。
*	MEF扩展：提供对MEF的配置、过滤。
*	扩展方法：提供字符串、日期、类型转换、中文处理、反射等常用的扩展方法。
*	线程共享：提供一个Scope本线程内数据共享，同时在 async / await 下也有效。
*	事件订阅：定义一组主题发布/事件订阅的接口，默认实现了基于队列和同步的订阅管理。
*	线程锁：提供读写锁、单例锁、异步锁和分布式锁的定义接口。
*	反射缓存：使用缓存提供反射调用的性能。
*	本地化：定义一组本地化方案，可使用基于资源文件或XML文件来配置字符串本地化资源。
*	时间监视器：提供一个用于记录方法执行耗时的计时器。
*	任务调度器：提供一个在后台定时运行任务的调度器。
*	线程锁/异步锁：提供线程锁/异步锁/分布式锁。

<b>`Fireasy.Data`</b>

数据库底层的访问类库，目前支持MsSql、Oracle、MySql、SQLite、PostgreSql、Firebird等常见数据库。
*	实例配置：可在同一应用中提供多个实例配置，以使可以访问不同的数据库，额外提供xml、binary、注册表等多种配置。另外还提供集群的配置方式，可实现读写分离。
*	适配器：IProvider用于适配不同的数据库类型，用于获取DbProviderFactory对象，以获得操作不同数据库的能力。该适配器通过反射的方式自动适配比较流行的驱动程序，你只需使用nuget引入相应的包即可，fireasy库里本身没有做强引用。
*	数据库操作：IDatabase提供ADO.NET的各种方法，如填充DataSet、返回Enumerable（包括T和dynamic）、返回DataReader、Update更新DataTable、事务操作等方法。支持读写分离。
*	参数化：统一使用@作为前缀，编码时不需要考虑数据库的差异，它会根据数据库类型进行自动转换，同时，现在也支持 in (@array) 这种集合类型的参数值。
*	分页参数：DataPager用于查询时指定分页的相关参数。
*	数据库工厂：在基于实例配置的前提下，DatabaseFactory能够返回与配置相符的IDatabase对象。
*	线程间共享：在同一线程内，如果你在子方法内使用数据库工厂创建实例，皆为外层创建的同一个实例，避免了资源的重复创建。
*	事务嵌套：如果外层已经开启事务的情况下，子方法再次开启事务将自动忽略，提交事务也只有在外层才有效，保证事务的一致笥。
*	命令追踪：在连接字符串里配置tracking即可以追踪数据访问对象所执行的每一条命令，以及执行命令耗用的时间。
*	提供者扩展：对适配器进行插件式扩展。包括：数据备份扩展、数据批量插入扩展、数据记录包装扩展、数据架构扩展、语法扩展和生成器扩展。
*	数据备份扩展：目前只实现了MsSql的备份。
*	数据批量插入扩展：实现每一种数据库类型的批量插入，该批量插入不是一般的insert，它实现万级数据的秒插。
*	数据记录包装扩展：针对每一种数据库类型，对IDataReader读取器读取各种数据数据类型进行包装，保证数据类型之间的一致性
*	数据架构扩展：提供对元数据的查询功能，可针对不同的数据库类型，返回数据库、用户、数据表、数据字段、存储过程、外键、索引等详细信息。
*	语法扩展：用于整合各数据库在语法上存在的差异，提供参数前缀、 逃逸符、数据类型转换、分页格式化处理、字符串连接、空判断、自编号脚本，以及各种字段串函数、日期函数、数学函数。
*	生成器扩展：对于没有自增特性的数据库，如oracle，可以使用序列生成主键值，另外还提供了基于公共用的方式提供序列值。
*	富类型转换：可以将Color、Image、Font、Point、Exception等对象放到库中，并且从库中读取。这些都是可配置的，可以自己进行扩展。
*	分页评估：提供大数据量和小数据量场景下的分页计算方法。默认的是使用TotalRecordEvaluato方式，如果数据量比较大，可以使用TryNextEvaluator方式。
*	排序转换器：排序时，用于将前端的字段与数据库字段进行转换对应。

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
*	依赖属性：你所看不到的又不得不了解的东西，它的存在为为了能够及时通知属性值的变更，属性的赋值与取值都跟它息息相关。
*	实体属性及实体集属性：和ef里的导航属性是一个概念，目的是为了实现关联查询。
*	实体模型：IEntity定义了一组实体的特性，正如上面说的一样，GetValue和SetValue离不开依赖属性，IsModified可判断属性值是否改变。LightEntity是目前所采用的基类，它在EntityContext初始化时，自动进行了AOP代理包装，使实体获得了状态记忆功能。因此你的属性需要定义成virtual。
*	实体关系：与ef类似，用于定义实体间的关系，主外键名称一致的情况下会自动创建关系，否则需要使用RelationshipAttribute或RelationshipAssignAttribute关联。
*	实体上下文：提供类似于ef的数据上下文，即EntityContext，它一样可以使用CodeFirst模式。
*	实体上下文对象池：可以使用对象池，提高对象的复用率降低创建对象耗用的时间。
*	实体仓储：每一个实体对应一个仓储，它们分布在EntityContext上，提供LINQ查询、新增、修改、删除等提供。
*	仓储适配器：默认的适配器是基于Fireasy.Data的，你也可以实现其他的适配器，以达到在不改变Entity模型的情况下依然可以使用其他框架的目的，比如ef、mongodb等等。
*	LINQ查询：支持常用的LINQ查询，如Where、OrderBy、GroupBy、Join等等。
*	逻辑删除标记：实体设置逻辑删除标记后，查询时将自动过滤这些已经被标记的数据。
*	全局筛选：Apply方法可以定义针对某实体类型的全局筛选条件。
*	延迟加载：对于关联属性，可以在需要的时候才从库中读取加载。
*	惰性加载：对于关联属性，由于延迟加载机制将发生n+1次数据库查询动作，此时可以使用Include方法将关联属性预先加载出来。
*	扩展方法：对LINQ查询的扩展，比较常用的如Segment、AssertWhere、ExtendAs、BatchOr、BatchAnd、CacheParsing等等，具体的使用不在这里说明。
*	树结构：采用类似00010001的编码来管理树结构，提供插入、移动、枚举孩子、递归父亲、获取兄弟等方法。
*	数据验证：基于DataAnnotations制定实体的数据验证规则。
*	持久化事务：基于Scope定义线程内的事务控制。
*	持久化环境：根据环境内的参数，格式化实体所映射的表名称，实现数据表横向扩展。
*	持久化事件订阅：提供一个订阅器，用于接收持久化事件，如新增、修改、删除等，事件分为Before和After，Before还可以对该操作进行取消。
*	动态持久化：通过动态构造实体类型，实现其持久化操作。
*	解析缓存：LINQ解析时，可以通过配置开启自动缓存开关（默认开），开启后，LINQ的解析时间将会有效缩短，该缓存存放于内存中，部分LINQ解析缓存存在问题，可在查询中使用CacheParsing关闭。
*	查询缓存：LINQ查询返回数据时，可以通过配置开启自动缓存开关（默认关），缓存是由Fireasy.Common缓存管理器提供的（最好是配置redis）。也可以在查询中使用CacheExecution指定是否开启查询缓存。
*	自定义函数：在LINQ中使用自定义方法，该方法与数据库的自定义函数相对应。
*	方法绑定：可在LINQ中自定义方法，对该方法进行绑定，即转换为可解析的Lambda表达式。

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
QQ群： 6406277、225698098
------------------------------------------------------------------------

![](http://www.fireasy.cn/content/images/Donate_fireasy.png)

