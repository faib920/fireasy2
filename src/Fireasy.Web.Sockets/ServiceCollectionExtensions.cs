#if NETSTANDARD
using Fireasy.Web.Sockets;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加分布式客户端管理器。
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWebSocketDistributedClientManager(this IServiceCollection services)
        {
            return services.AddSingleton<IClientManager, DistributedClientManager>();
        }

        /// <summary>
        /// 添加自定义的客户端管理器。
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWebSocketClientManager<T>(this IServiceCollection services) where T : IClientManager
        {
            return services.AddSingleton(typeof(IClientManager), typeof(T));
        }
    }
}
#endif