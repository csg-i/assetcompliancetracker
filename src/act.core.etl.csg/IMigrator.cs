using System.Threading.Tasks;

namespace act.core.etl.csg
{
    public interface IMigrator
    {
        Task<int> AddOrUpdateNodes(int pageIndex);
        Task<int> AddOrUpdateProducts();
        Task<int> AddOrUpdateFunctions();
        Task<int> AddOrUpdateEmployees(int pageIndex);
        Task<int> GetNodePages();
        Task<int> GetEmployeePages();
        Task<int> AddorUpdateEnvironments();
    }
}