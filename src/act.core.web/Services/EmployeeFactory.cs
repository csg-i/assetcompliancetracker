using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Models.Employees;
using Microsoft.EntityFrameworkCore;

namespace act.core.web.Services
{
    public interface IEmployeeFactory
    {
        Task<JsonEmployeeSearchResult[]> TypeAheadSearch(string query);
    }

    internal class EmployeeFactory : IEmployeeFactory
    {
        private readonly ActDbContext _ctx;

        public EmployeeFactory(ActDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<JsonEmployeeSearchResult[]> TypeAheadSearch(string query)
        {
            return (await _ctx.Employees.AsNoTracking()
                    .Search(query)
                    .OrderBy(p => p.PreferredName ?? p.FirstName)
                    .ThenBy(p => p.LastName)
                    .Take(10)
                    .ToArrayAsync())
                .Select(p => new JsonEmployeeSearchResult(p.Id, p.OwnerText()))
                .ToArray();
        }
    }
}