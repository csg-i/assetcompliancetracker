using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using act.core.etl.lambda;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace act.core.etl.csg
{
    public class LambdaAddin:LambdaAddinBase
    {
        
        public override void AddServices(IServiceCollection services)
        {
            services    
                .AddAWSService<IAmazonS3>()                
                .AddTransient<IMigrator, Migrator>()
                .AddTransient<IExporter, Exporter>();
        }

        public override IDictionary<string, Func<IServiceScope, Argument, Task<int>>> ProcessFunctions { get; } =

            new Dictionary<string, Func<IServiceScope, Argument, Task<int>>>
            {
                {
                    "environments",
                    async (scope, args) => await scope.ServiceProvider.GetService<IMigrator>().AddorUpdateEnvironments()
                },
                {
                    "products",
                    async (scope, args) => await scope.ServiceProvider.GetService<IMigrator>().AddOrUpdateProducts()
                },
                {
                    "functions",
                    async (scope, args) => await scope.ServiceProvider.GetService<IMigrator>().AddOrUpdateFunctions()
                },
                {
                    "employees",
                    async (scope, args) =>
                        await scope.ServiceProvider.GetService<IMigrator>().AddOrUpdateEmployees(args.Index)
                },
                {
                    "nodes",
                    async (scope, args) =>
                        await scope.ServiceProvider.GetService<IMigrator>().AddOrUpdateNodes(@args.Index)
                },
                {
                    "employeepages",
                    async (scope, args) => await scope.ServiceProvider.GetService<IMigrator>().GetEmployeePages()
                },
                {
                    "nodepages",
                    async (scope, args) => await scope.ServiceProvider.GetService<IMigrator>().GetNodePages()
                },
                {
                    "employeesall", async (scope, args) =>
                    {
                        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
                        var emps = Enumerable.Range(0, await migrator.GetEmployeePages()).ToArray();
                        var lambda = scope.ServiceProvider.GetRequiredService<IAmazonLambda>();
                        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        RecursiveLambdaCall(lambda, config.GetValue<string>("FunctionName"), emps,
                            "employees");

                        return emps.Max();
                    }
                },
                {
                    "nodesall", async (scope, args) =>
                    {
                        var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
                        var nodes = Enumerable.Range(0, await migrator.GetNodePages()).ToArray();
                        var lambda = scope.ServiceProvider.GetRequiredService<IAmazonLambda>();
                        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        RecursiveLambdaCall(lambda,
                            (config.GetValue<string>("FunctionName") ?? "aaw-act-migrator").ToLower(), nodes, "nodes");

                        return nodes.Max();
                    }
                },
                {
                    "dumpnodes", async (scope, args) =>
                    {
                        var data = await scope.ServiceProvider.GetService<IExporter>().Nodes();
                        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        await WriteToS3(scope.ServiceProvider.GetService<IAmazonS3>(),
                            (config.GetValue<string>("Bucket") ?? "asaactdump").ToLower(), "nodes.json", data);
                        return data.Length;
                    }
                },
                {
                    "dumpsoftware", async (scope, args) =>
                    {
                        var software = await scope.ServiceProvider.GetService<IExporter>().Software();
                        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        await WriteToS3(scope.ServiceProvider.GetService<IAmazonS3>(),
                            (config.GetValue<string>("Bucket") ?? "asaactdump").ToLower(), "software.json", software);
                        return software.Count;
                    }
                }
            };

        private static void RecursiveLambdaCall(IAmazonLambda lambda, string functionName, int[] indexes, string name)
        {
            foreach (var index in indexes)
            {
                var payload = JsonConvert.SerializeObject (new Argument { Index = index, Name = name });
                var request = new InvokeRequest
                {
                    FunctionName = functionName,
                    Payload = payload
                };
                Task.Run(() => lambda.InvokeAsync(request));
                LambdaLogger.Log($"Kicked off request ${payload}");
                Thread.Sleep(1500);
            }
        }

        private static async Task WriteToS3(IAmazonS3 s3, string bucket, string file, object data)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            await s3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucket,
                Key = file,
                ContentBody = json,
                ContentType = "application/json"
            });
        }
        
    }
}