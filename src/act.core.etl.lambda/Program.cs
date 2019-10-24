using System.Threading.Tasks;

namespace act.core.etl.lambda
{
    public class Program
    {
        static async Task Main()
        {
            var mig = new Migrator();
#if DEBUG
            await mig.Migrate(new Argument {Name = "products"});
#else
            await mig.RunLambda();
#endif
        }
    }
}