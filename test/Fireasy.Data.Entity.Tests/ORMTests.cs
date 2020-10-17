using Fireasy.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSugar;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Fireasy.Data.Entity.Linq;
using Fireasy.Data.Entity.Properties;

namespace Fireasy.Data.Entity.Tests
{
    [TestClass]
    public class ORMTests
    {
        static string connstr = "data source=(local);user id=sa;password=123;initial catalog=test";

        [SugarTable("SysUser")]
        [Table("SysUser")]
        public class SysUser : LightEntity<SysUser>
        {
            [Key]
            public virtual int UserID { get; set; }

            public virtual int OrgID { get; set; }

            public virtual string Name { get; set; }

            public virtual string Account { get; set; }

            public virtual string PostNames { get; set; }

            public virtual string Password { get; set; }

            public virtual string Mobile { get; set; }

            public virtual string Email { get; set; }

            public virtual string IDCard { get; set; }

            public virtual int Sex { get; set; }

            public virtual int? DegreeNo { get; set; }

            public virtual int? TitleNo { get; set; }

            public virtual string PyCode { get; set; }

            public virtual int State { get; set; }

            public virtual DateTime? LastLoginTime { get; set; }

            public virtual bool IsDriver { get; set; }

            public virtual string DriverNo { get; set; }

            public virtual string Token { get; set; }

            public virtual string DeviceNo { get; set; }
        }

        public class FireasyDbContext : EntityContext
        {
            protected override void OnConfiguring(EntityContextOptionsBuilder builder)
            {
                var map = new PropertyMapInfo { IsPrimaryKey = true, DataType = System.Data.DbType.Boolean, Description = "dd" };
                var p = new GeneralProperty { Info = map };
                PropertyUnity.RegisterProperty(typeof(SysUser), p);
                builder.UseSqlServer(connstr);
            }

            public EntityRepository<SysUser> Users { get; set; }
        }

#if NETSTANDARD2
        public class EfDbContext : Microsoft.EntityFrameworkCore.DbContext
        {
            protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
            {
                Microsoft.EntityFrameworkCore.SqlServerDbContextOptionsExtensions.UseSqlServer(optionsBuilder, connstr);
            }

            public Microsoft.EntityFrameworkCore.DbSet<SysUser> Users { get; set; }
        }
#endif

        public ORMTests()
        {
            InitConfig.Init();
        }

        [TestMethod]
        public void TestQuerySqlSugarAll()
        {
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connstr,
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true
            });

            var t = TimeWatcher.Watch(() =>
            {
                var list1 = db.Queryable<SysUser>().ToList();
            });

            Console.WriteLine(t);
        }

        [TestMethod]
        public void TestQueryFireasyAll()
        {
            using (var db = new FireasyDbContext())
            {
                var t = TimeWatcher.Watch(() =>
                {
                    var list1 = db.Users.Where(s => s.UserID != 0).AsNoTracking().ToList();
                });

                Console.WriteLine(t);
            }
        }

#if NETSTANDARD
        [TestMethod]
        public void TestQueryEFAll()
        {
            using (var db = new EfDbContext())
            {
                var t = TimeWatcher.Watch(() =>
                {
                    var list1 = db.Users.Where(s => s.UserID != 0).ToList();
                    list1[0].Name = "aa";
                });

                db.SaveChanges();

                Console.WriteLine(t);
            }
        }
#endif
    }
}
