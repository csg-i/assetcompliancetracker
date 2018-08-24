using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace act.core.etl
{
    public class Argument
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public interface IArgumentProcessor
    {
        Task<int> Process(Argument argument);
    }

    internal class ArgumentProcessor : IArgumentProcessor
    {
        private readonly ConcurrentDictionary<string, Func<IServiceScope, Argument, Task<int>>> _registry =
            new ConcurrentDictionary<string, Func<IServiceScope, Argument, Task<int>>>();

        public ArgumentProcessor(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        private IServiceProvider ServiceProvider { get; }

        public async Task<int> Process(Argument argument)
        {
            if (string.IsNullOrWhiteSpace(argument?.Name) || !_registry.ContainsKey(argument.Name))
                return 0;

            using (var scope = ServiceProvider.CreateScope())
            {
                return await _registry[argument.Name].Invoke(scope, argument);
            }
        }


        public ArgumentProcessor RegisterProcessorFunction(string name, Func<IServiceScope, Argument, Task<int>> task)
        {
            _registry.TryAdd(name ?? throw new ArgumentNullException(nameof(name)),
                task ?? throw new ArgumentNullException(nameof(task)));
            return this;
        }

        public void RegisterProcessorFunctions(
            params IDictionary<string, Func<IServiceScope, Argument, Task<int>>>[] dictionary)
        {
            if (dictionary == null || dictionary.Length == 0)
                return;

            foreach (var d in dictionary)
                if (d != null && d.Count > 0)
                    foreach (var kvp in d)
                        RegisterProcessorFunction(kvp.Key, kvp.Value);
        }
    }
}