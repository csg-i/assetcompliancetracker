using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace act.core.etl.lambda
{
    public class Migrator : IDisposable
    {
        private ServiceProvider _serviceProvider;

        private const string DefaultParameterStoreValue = "/ActLambda";
        private const string ParameterStoreName = "ConnectionStringParameterQuery";
        public Migrator()
        {
            var parm = DefaultParameterStoreValue;
            var env = Environment.GetEnvironmentVariables();
            if (env.Contains(ParameterStoreName) && env[ParameterStoreName] != null)
                parm = env[ParameterStoreName].ToString();   
            
            
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .AddSystemsManager(parm)
                .Build();
            var addins = config.GetSection("AddIns").GetChildren().Select(p => p.Value).ToArray();
            
            var services = new ServiceCollection().ConfigureLambda(config).ConfigureLambdaArgumentProcessor();
            var all = new List<IDictionary<string, Func<IServiceScope, Argument, Task<int>>>>();
            var runningPath = Path.GetDirectoryName(GetType().Assembly.Location);
            var addinBase = typeof(LambdaAddinBase);
            if (addins.Length > 0)
            {
                foreach (var addin in addins)
                {
                    var ass = AssemblyLoader.LoadFromAssemblyPath(
                        Path.Combine(runningPath, $"{addin}.dll"));
                    var assTypes =
                        ass.DefinedTypes.Where(p => p.IsSubclassOf(addinBase) && p.IsPublic && !p.IsAbstract);
                    var loaders = assTypes.ToArray();
                    foreach (var loader in loaders)
                    {
                        if (loader.IsSubclassOf(addinBase))
                        {
                            if (Activator.CreateInstance(loader) is LambdaAddinBase it)
                            {
                                it.AddServices(services);
                                all.Add(it.ProcessFunctions);
                            }
                        }
                    }
                }
            }
            
           
            services.ConfigureLambdaArgumentProcessor(all.ToArray());

            _serviceProvider = services.BuildServiceProvider();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            _serviceProvider = null;
        }

        [LambdaSerializer(typeof(JsonSerializer))]
        public async Task<int> Migrate(Argument @event)
        {
            if (@event?.Name == null)
                return 0;

            LambdaLogger.Log($"Migrate called with argument {@event.Name} and index {@event.Index}");
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var ret =  await scope.ServiceProvider.GetRequiredService<IArgumentProcessor>().Process(@event);

                    LambdaLogger.Log($"Return {ret} when migrate called with argument {@event.Name} and index {@event.Index}.");
                    return ret;
                }
                catch (Exception ex)
                {
                    LambdaLogger.Log($"Exception occurred for {@event.Name} with index {@event.Index} of : {ex}");
                    throw;
                }
            }
        }
    }
}