using System.Threading.Tasks;
using act.core.web.Framework;
using act.core.web.Models;

namespace act.core.web.Services
{
    public interface ISpecificationFactory<TInfo, TSearchResult> where TInfo : class, ISpecInformation
        where TSearchResult : class, ISpecSearchResult
    {
        Task AddOrUpdate(TInfo info);
        Task<long> Clone(long id, IUserSecurity employeeSecurity);
        Task<string> Delete(long id, IUserSecurity user);
        Task<TInfo> GetOne(long id);

        Task<TSearchResult[]> GetSearchResults(SpecSearchTypeConstant type, string query,
            IUserSecurity employeeSecurity);

        Task<bool> IsUnique(TInfo info);
        Task<JsonSpecSearchResult[]> TypeAheadSearch(string query);
    }
}