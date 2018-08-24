using System.Linq;

namespace act.core.etl.lambda
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var it = new Migrator();
            it.Migrate(new Argument { Index = 0, Name = "environments" }).GetAwaiter().GetResult();
            it.Migrate(new Argument { Index = 0, Name = "products" }).GetAwaiter().GetResult();
            it.Migrate(new Argument { Index = 0, Name = "functions" }).GetAwaiter().GetResult();
            
            var empPages =  it.Migrate(new Argument { Index = 0, Name = "employeepages" }).GetAwaiter().GetResult();
            var empRange = Enumerable.Range(0, empPages).ToArray();
            foreach (var empPage in empRange)
            {
                it.Migrate(new Argument {Index = empPage, Name = "employees"}).GetAwaiter().GetResult();
            }


            var nodePages =  it.Migrate(new Argument { Index = 0, Name = "nodepages" }).GetAwaiter().GetResult();
            var nodeRange = Enumerable.Range(0, nodePages).ToArray();
            foreach (var nodePage in nodeRange)
            {
                it.Migrate(new Argument {Index = nodePage, Name = "nodes"}).GetAwaiter().GetResult();
            }
            
        }
    }
}