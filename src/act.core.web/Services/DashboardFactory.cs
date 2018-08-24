using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Models.Dashboard;
using act.core.web.Models.ScoreCard;
using Microsoft.EntityFrameworkCore;

namespace act.core.web.Services
{
    public interface IDashboardFactory
    {
        Task<Spread> Spread();
        Task<TopOffenders> TopOffenders();
        Task<Status> Status();
        Task<DirectorScore> GetDirectorPciScoreChart();
        Task<ComplianceOverTime> ComplianceOverTime(int daysBack, bool pciOnly, long? employeeId, ComplianceOverTime.EmployeeFilterConstant filterType);
    }

    public class DashboardFactory : IDashboardFactory
    {
        private readonly ActDbContext _ctx;

        public DashboardFactory(ActDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<DirectorScore> GetDirectorPciScoreChart()
        {
            var directors = await _ctx.Nodes.AsNoTracking().Active().Where(p => p.Owner.ReportingDirectorId != null)
                .Select(p => p.Owner.ReportingDirector).Distinct()
                .OrderBy(p => p.PreferredName ?? p.FirstName).ThenBy(p => p.LastName)
                .ToDictionaryAsync(p => p.Id, p => p);
            directors.Add(-1,
                new Employee
                {
                    Id = -1,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    PreferredName = "(Not Found)"
                });
            var names = new List<string>();
            var passing = new List<int>();
            var failing = new List<int>();
            var notReporting = new List<int>();
            foreach (var director in directors.Keys)
            {
                var an = _ctx.Nodes.AsNoTracking().ByDirector(director == -1 ? null : (int?) director).Active()
                    .ProductIsNotExlcuded().InPciScope().InChefScope().Assigned();
                if (await an.AnyAsync())
                {
                    names.Add(directors[director].OwnerText(false));
                    passing.Add(await an.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope()
                        .CountAsync());
                    failing.Add(await an.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope()
                        .CountAsync());
                    notReporting.Add(await an.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope()
                        .CountAsync());
                }
            }

            return new DirectorScore(names.ToArray(), passing.ToArray(), failing.ToArray(), notReporting.ToArray());
        }

        public async Task<ComplianceOverTime> ComplianceOverTime(int daysBack, bool pciOnly, long? employeeId, ComplianceOverTime.EmployeeFilterConstant filterType)
        {
            var today = DateTime.Today.AddDays(1);
            var start = today.AddDays(-1 * daysBack);
            
            
        
            var statusConstants = Enum.GetValues(typeof(ComplianceStatusConstant)).Cast<ComplianceStatusConstant>()
                .ToArray();

            var maxQuery = _ctx.ComplianceResults.AsNoTracking().Active().ProductIsNotExlcuded().InChefScope().Where(r =>
                r.EndDate >= start && r.EndDate <= today);

            if (pciOnly)
                maxQuery = maxQuery.InPciScope();

            if (employeeId.HasValue)
            {
                switch (filterType)
                {
                        case Models.Dashboard.ComplianceOverTime.EmployeeFilterConstant.Director:
                            maxQuery = maxQuery.Where(r => r.Node.Owner.ReportingDirectorId == employeeId);
                            break;                            
                        case Models.Dashboard.ComplianceOverTime.EmployeeFilterConstant.Owner:
                            maxQuery = maxQuery.Where(r => r.Node.OwnerEmployeeId == employeeId);                            
                            break;
                        default:
                            throw new ArgumentException($"The filter type {filterType} is not handled.", nameof(filterType));
                }
            }

            var inner = maxQuery.GroupBy(r => new {r.EndDate, r.InventoryItemId})
                .Select(g => new {g.Key.InventoryItemId, g.Key.EndDate, Id =g.Max(r => r.Id)});

            var dayCount = (await _ctx.ComplianceResults.AsNoTracking().Join(
                    inner,
                    r => r.Id,
                    i => i.Id,
                    (r, i) => r
                ).GroupBy(p => new {p.EndDate, p.Status})
                .Select(g => new {g.Key.EndDate, g.Key.Status, Count = g.Count()})
                .ToArrayAsync()).Select(p => new ComplianceOverTimeQueryResult(p.EndDate, p.Status, p.Count)).ToArray();
            
            var allDates = Enumerable.Range(0, daysBack).Select(p => new ComplianceOverTimeData(p, start.AddDays(p)))
                .ToDictionary(p => p.Date, p => p);
            
            var keys = allDates.Keys;
            
            foreach (var key in keys)
            {
                foreach (var compliance in statusConstants)
                    allDates[key].Counts.Add(compliance,
                       dayCount.Where(p => p.EndDate == key && p.Status == compliance).Select(p => p.Count).Sum());
            }

            return new ComplianceOverTime(allDates);
        }

        public async Task<Spread> Spread()
        {

            var osSpecs = (await _ctx.BuildSpecifications.AsNoTracking()
                    .Where(b => b.BuildSpecificationType == BuildSpecificationTypeConstant.OperatingSystem)
                    .GroupBy(b => new {b.OperatingSystemName, b.OperatingSystemVersion})
                    .Select(g => new {g.Key.OperatingSystemName, g.Key.OperatingSystemVersion, Count = g.Count()})
                    .OrderBy(b => b.OperatingSystemName)
                    .ThenBy(b => b.OperatingSystemVersion)
                    .ToArrayAsync()
                ) //yank from db
                .ToDictionary(p => $"{p.OperatingSystemName} @ {p.OperatingSystemVersion}", p => p.Count);


            var osNodes = (await _ctx.Nodes.AsNoTracking().Active().Assigned()
                    .GroupBy(n => new
                    {
                        n.BuildSpecification.Parent.OperatingSystemName,
                        n.BuildSpecification.Parent.OperatingSystemVersion
                    })
                    .Select(g => new {g.Key.OperatingSystemName, g.Key.OperatingSystemVersion, Count = g.Count()})
                    .OrderBy(b => b.OperatingSystemName)
                    .ThenBy(b => b.OperatingSystemVersion)
                    .ToArrayAsync()
                ) //yank from db
                .ToDictionary(p => $"{p.OperatingSystemName} @ {p.OperatingSystemVersion}", p => p.Count);

            var nodePart = _ctx.Nodes.AsNoTracking().Active().ProductIsNotExlcuded().Assigned();
            
            var appSpecs = (await nodePart
                    .Select(g => new {g.BuildSpecificationId, g.ProductCode, g.FunctionId})
                    .Distinct()
                    .Join(_ctx.Products,
                        b=>b.ProductCode,
                        p=>p.Code,
                        (b,p)=> new{b.BuildSpecificationId, Product = p.Name, b.FunctionId}
                        )
                    .Join(_ctx.Functions,
                        b=>b.FunctionId,
                        f=>f.Id,
                        (b,f)=> new{b.BuildSpecificationId, b.Product, Function = f.Name}
                    )
                    .GroupBy(p=> new{p.Product, p.Function})
                    .Select(g => new {g.Key.Product, g.Key.Function, Count = g.Count()})
                    .ToArrayAsync()
                ) //yank from db
                .ToDictionary(p => $"{p.Product} - {p.Function}", p => p.Count);
           
            var appNodes = (await nodePart
                    .GroupBy(p => new {Product = p.Product.Name, Function = p.Function.Name})
                    .Select(g => new {g.Key.Product, g.Key.Function, Count = g.Count()})
                    .OrderBy(p => p.Product)
                    .ThenBy(p => p.Function)
                    .ToArrayAsync()
                ) //yank from db
                .ToDictionary(p => $"{p.Product} - {p.Function}", p => p.Count);

            return new Spread(Combine(osSpecs, osNodes), Combine(appSpecs, appNodes));
        }

        public async Task<TopOffenders> TopOffenders()
        {
            var yestarday = DateTime.Today.AddDays(-1);

            var osFailures = await _ctx.ComplianceResults.AsNoTracking().Where(p => p.EndDate > yestarday)
                .Active().InChefScope().InPciScope().ProductIsNotExlcuded()
                .CountAsync(p => !p.OperatingSystemTestPassed);

            var top10 = (await _ctx.ComplianceResultTests.AsNoTracking()
                    .Active()
                    .InPciScope()
                    .InChefScope()
                    .ProductIsNotExlcuded()
                    .Where(t => t.ComplianceResult.EndDate > yestarday)
                    .GroupBy(t => new {t.ResultType, t.ShouldExist, t.PortType, t.Name})
                    .Select(g =>
                        new {g.Key.ResultType, g.Key.ShouldExist, g.Key.PortType, g.Key.Name, Count = g.Count()})
                    .OrderByDescending(t => t.Count)
                    .Take(10)
                    .ToArrayAsync()
                ) //yank from db
                .Select(t => new Offender(t.ResultType, t.ShouldExist, t.PortType, t.Name, t.Count));
            return new TopOffenders(osFailures, top10);
        }

        public async Task<Status> Status()
        {
            var active = _ctx.Nodes.AsNoTracking().Active();
            var inScope = active.ProductIsNotExlcuded().InChefScope();


            var status = inScope.Assigned();
            var unix = active.ByPlatform(PlatformConstant.Unix);
            var appliance = active.ByPlatform(PlatformConstant.Appliance);
            var osOther = active.ByPlatform(PlatformConstant.Other);
            var productExclude = active.InChefScope().ProductIsExlcuded();

            return new Status(
                new ScoreCardPciCount(
                    await status.InPciScope().ByComplianceStatus(ComplianceStatusConstant.Succeeded).CountAsync(),
                    await status.ByComplianceStatus(ComplianceStatusConstant.Succeeded).CountAsync()),
                new ScoreCardPciCount(
                    await status.InPciScope().ByComplianceStatus(ComplianceStatusConstant.Failed).CountAsync(),
                    await status.ByComplianceStatus(ComplianceStatusConstant.Failed).CountAsync()),
                new ScoreCardPciCount(
                    await inScope.Unassigned().InPciScope().CountAsync(),
                    await inScope.Unassigned().CountAsync()),
                new ScoreCardPciCount(
                    await status.InPciScope().ByComplianceStatus(ComplianceStatusConstant.NotFound).CountAsync(),
                    await status.ByComplianceStatus(ComplianceStatusConstant.NotFound).CountAsync()),
                new ScoreCardPciCount(
                    await unix.InPciScope().CountAsync(),
                    await unix.CountAsync()),
                new ScoreCardPciCount(
                    await osOther.InPciScope().CountAsync(),
                    await osOther.CountAsync()),
                new ScoreCardPciCount(
                    await appliance.InPciScope().CountAsync(),
                    await appliance.CountAsync()),
                new ScoreCardPciCount(
                    await productExclude.InPciScope().CountAsync(),
                    await productExclude.CountAsync())
            );
        }

        private static void PopluateMissing(IDictionary<string, int> dict, IEnumerable<string> keys)
        {
            var missing = keys.Except(dict.Keys);
            foreach (var m in missing) dict.Add(m, 0);
        }

        private static Stat[] Combine(IDictionary<string, int> specs, IDictionary<string, int> nodes)
        {
            PopluateMissing(specs, nodes.Keys.ToArray());
            PopluateMissing(nodes, specs.Keys.ToArray());
            return specs.Join(nodes, s => s.Key, n => n.Key, (s, n) => new Stat(s.Key, s.Value, n.Value)).ToArray();
        }

        private class ComplianceOverTimeQueryResult
        {
            public ComplianceOverTimeQueryResult(DateTime endDate, ComplianceStatusConstant status, int count)
            {
                EndDate = endDate;
                Status = status;
                Count = count;
            }

            public DateTime EndDate { get; }
            public ComplianceStatusConstant Status { get; }
            public int Count { get; }
        }
    }
}