using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace act.core.etl.lambda
{
    public abstract class LambdaAddinBase
    {
        public abstract IDictionary<string, Func<IServiceScope, Argument, Task<int>>> ProcessFunctions { get; }
        public abstract void AddServices(IServiceCollection services);
    }
}