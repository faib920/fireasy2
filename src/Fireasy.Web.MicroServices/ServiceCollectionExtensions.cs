using Fireasy.Web.MicroServices;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMicroService(this IServiceCollection services, string url)
        {
            var reg = new ServiceRegistration { Url = url };
            return services;
        }
    }
}
