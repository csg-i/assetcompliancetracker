using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace act.core.etl
{
    public static class Bootstrap
    {
        public static IServiceCollection ConfigureGatherer(this IServiceCollection services)
        {
            return services.AddTransient<IGatherer, Gatherer>();
        }     
               
    }
}