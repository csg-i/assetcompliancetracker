using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace act.core.data
{
    public static class Bootstrap
    {
        public static IServiceCollection AddActDbContext(this IServiceCollection services, IConfiguration configuration,  int commandTimeout=30, string fallbackConnectionStringName="ActDb")
        {
            var aurora = new AuroraAwareConnectionStringBuilder(configuration);
            return services.AddDbContext<ActDbContext>(options =>

                options.UseMySql(aurora.GetConnectionString(fallbackConnectionStringName), o =>
                    {
                        o.ServerVersion(new Version(5, 6, 10), ServerType.MySql);
                        o.CommandTimeout(commandTimeout);
                    })
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning)));

        }
        public static IServiceCollection AddActDbContextPool(this IServiceCollection services, IConfiguration configuration, int commandTimeout=30, string fallbackConnectionStringName = "ActDb")
        {
            var aurora = new AuroraAwareConnectionStringBuilder(configuration);
            return services.AddDbContextPool<ActDbContext>(options =>
                options.UseMySql(aurora.GetConnectionString(fallbackConnectionStringName), o =>
                    {
                        o.ServerVersion(new Version(5, 6, 10), ServerType.MySql);
                        o.CommandTimeout(commandTimeout);
                    })
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning)));
        }
    }
}