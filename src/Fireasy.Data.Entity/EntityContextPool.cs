// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using Fireasy.Common.MultiTenancy;
using System;
using System.Linq;
using System.Reflection;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// <see cref="EntityContext{TContext}"/> 的缓存池。
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class EntityContextPool<TContext> : ObjectPool<TContext> where TContext : EntityContext
    {
        /// <summary>
        /// 初始化 <see cref="EntityContextPool{TContext}"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="maxSize">缓冲池的最大数量。</param>
        public EntityContextPool(IServiceProvider serviceProvider, int maxSize)
            : base(serviceProvider, null, maxSize)
        {
        }

        /// <summary>
        /// 从缓冲池拿出一个 <typeparamref name="TContext"/> 对象。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public TContext Rent(IServiceProvider serviceProvider)
        {
            return base.Rent(() => CreateInstance(serviceProvider), s => s.TrySetServiceProvider(serviceProvider));
        }

        /// <summary>
        /// 从缓冲池中取得一个对象。
        /// </summary>
        public sealed class Lease : DisposeableBase
        {
            private EntityContextPool<TContext> _pool;

            /// <summary>
            /// 初始化 <see cref="Lease"/> 类的新实例。
            /// </summary>
            /// <param name="pool"></param>
            public Lease(IServiceProvider serviceProvider, EntityContextPool<TContext> pool)
            {
                _pool = pool;

                Context = _pool.Rent(serviceProvider);
            }

            /// <summary>
            /// 返回当前的 <see cref="TContext"/> 对象。
            /// </summary>
            public TContext Context { get; private set; }

            protected override bool Dispose(bool disposing)
            {
                if (_pool != null)
                {
                    //归还时如果缓冲池已满，则要进行销毁
                    if (!_pool.Return(Context))
                    {
                        ((IObjectPoolable)Context).SetPool(null);
                        Context.TryDispose();
                    }

                    _pool = null;
                    Context = null;
                }

                return base.Dispose(disposing);
            }
        }

        /// <summary>
        /// 创建一个新实例。
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static TContext CreateInstance(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.TryGetService<EntityContextOptions<TContext>>();

            var constructors = from s in typeof(TContext).GetTypeInfo().DeclaredConstructors
                               where !s.IsStatic && s.IsPublic
                               let pars = s.GetParameters()
                               orderby pars.Length descending
                               select new { info = s, pars };

            foreach (var cons in constructors)
            {
                var length = cons.pars.Length;
                var match = 0;
                var arguments = new object[length];
                for (var i = 0; i < length; i++)
                {
                    var parType = cons.pars[i].ParameterType;
                    if (typeof(IServiceProvider) == parType)
                    {
                        arguments[i] = serviceProvider;
                        match++;
                    }
                    else if (typeof(EntityContextOptions<TContext>) == parType ||
                        typeof(EntityContextOptions) == parType)
                    {
                        arguments[i] = options;
                        match++;
                    }
                    else
                    {
                        var svrArg = serviceProvider.GetService(parType);
                        if (svrArg != null)
                        {
                            arguments[i] = svrArg;
                            match++;
                        }
                    }
                }

                if (length == match)
                {
                    return (TContext)cons.info.FastInvoke(arguments);
                }
            }

            return null;
        }
    }
}
