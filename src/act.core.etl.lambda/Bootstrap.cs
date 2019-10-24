using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using act.core.data;
using Amazon.Lambda;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace act.core.etl.lambda
{
    internal static class Bootstrap
    {
        public static IServiceCollection ConfigureLambdaArgumentProcessor(this IServiceCollection services,
            params IDictionary<string, Func<IServiceScope, Argument, Task<int>>>[] dictionary)
        {
            return services
                .AddSingleton<IArgumentProcessor>(sp =>
                {
                    var it = new ArgumentProcessor(sp);
                    it.RegisterProcessorFunction("databaseupdate", async (scope, arg) =>
                        {
                            await scope.ServiceProvider.GetRequiredService<ActDbContext>().Database.MigrateAsync();
                            return 0;
                        })
                        .RegisterProcessorFunction("gather",
                            async (scope, arg) =>
                                (await scope.ServiceProvider.GetRequiredService<IGatherer>().Gather(arg.Index)).Length)
                        .RegisterProcessorFunction("email", async (scope, arg) =>
                        {
                            if (arg.Index == 1)
                                return await scope.ServiceProvider.GetService<IGatherer>().NotifyNotReportingNodes();
                            if (arg.Index == 0)
                                return await scope.ServiceProvider.GetService<IGatherer>().NotifyUnassignedNodes();
                            return 0;
                        })
                        .RegisterProcessorFunction("reset",
                            async (scope, arg) =>
                                await scope.ServiceProvider.GetService<IGatherer>().ResetComplianceStatus())
                        .RegisterProcessorFunction("purgedetails",
                            async (scope, arg) =>
                                await scope.ServiceProvider.GetService<IGatherer>().PurgeOldComplianceDetails())
                        .RegisterProcessorFunction("purgeruns",
                            async (scope, arg) =>
                                await scope.ServiceProvider.GetService<IGatherer>().PurgeOldComplianceRuns())
                        .RegisterProcessorFunction("purgeinactive",
                            async (scope, arg) =>
                                await scope.ServiceProvider.GetService<IGatherer>().PurgeInactiveNodes())
                        .RegisterProcessorFunctions(dictionary);

                    return it;
                });
        }

        public static IServiceCollection ConfigureLambda(this IServiceCollection services, IConfiguration config)
        {
            return services
                .AddSingleton(config)
                .AddLogging(opt =>
                {
                    opt.AddConfiguration(config);
                    opt.SetMinimumLevel(LogLevel.Debug);
                    opt.AddConsole();
                })
                .AddDefaultAWSOptions(config.GetAWSOptions())
                .AddAWSService<IAmazonLambda>()
                .AddActDbContext(config, 120)
                .ConfigureGatherer();
        }
    }
}