using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Models.ScoreCard;
using Microsoft.EntityFrameworkCore;

namespace act.core.web.Services
{
    internal class ScoreCardFactory : IScoreCardFactory
    {
        private readonly ActDbContext _ctx;

        public ScoreCardFactory(ActDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<OwnerScoreCard> GetOwnerScoreCard(long employeeId)
        {
            var emp = await _ctx.Employees.ById(employeeId);
            if (emp == null)
                return new OwnerScoreCard(employeeId, "NA", Enumerable.Empty<OwnerScoreCardRow>());

            var specs = await _ctx.BuildSpecifications
                .Where(p => p.OwnerEmployeeId == employeeId)
                .Select(p => new
                {
                    p.Id,
                    p.ParentId,
                    p.Name,
                    p.BuildSpecificationType,
                    Platform = p.BuildSpecificationType == BuildSpecificationTypeConstant.OperatingSystem
                        ? p.Platform
                        : p.Parent.Platform,
                    ParentName = p.ParentId.HasValue ? p.Parent.Name : string.Empty,
                    Ports = p.Ports.Count(),
                    Software = p.SoftwareComponents.Count(),
                    Unjustified = p.SoftwareComponents.Count(s=>s.JustificationId == null),
                    ParentPorts = p.ParentId.HasValue ? p.Parent.Ports.Count() : 0,
                    ParentSoftware = p.ParentId.HasValue ?p.Parent.SoftwareComponents.Count() : 0,
                    ParentUnjustified = p.ParentId.HasValue ?p.Parent.SoftwareComponents.Count(s=>s.JustificationId == null) : 0,
                    Nodes = p.Nodes.Count(),
                    Succeeded = p.Nodes.Count(s=>s.ComplianceStatus == ComplianceStatusConstant.Succeeded),
                    Failed = p.Nodes.Count(s=>s.ComplianceStatus == ComplianceStatusConstant.Failed)
                })
                .OrderBy(p => p.Name)
                .ToArrayAsync();

            var list = new List<OwnerScoreCardRow>();
            foreach (var spec in specs)
            {
                var row = new OwnerScoreCardRow(spec.Id,
                    spec.Name,
                    spec.ParentId,
                    spec.ParentName,
                    spec.BuildSpecificationType,
                    spec.Platform.GetValueOrDefault(PlatformConstant.Linux),
                    spec.Nodes,
                    spec.Succeeded,
                    spec.Failed,
                    new ScoreCardCount(spec.Software, spec.ParentSoftware),
                    new ScoreCardCount(spec.Unjustified, spec.ParentUnjustified),
                    new ScoreCardCount(spec.Ports, spec.ParentPorts));
                list.Add(row);
            }

            return new OwnerScoreCard(employeeId, emp.OwnerText(), list);
        }

        public async Task<ExecutiveScoreCard> GetExecutiveScoreCard(long employeeId)
        {
            var emp = await _ctx.Employees.Include(p => p.Supervisor).ById(employeeId);
            if (emp == null)
                return new ExecutiveScoreCard(employeeId, "NA", null, null,
                    Enumerable.Empty<ExecutiveScoreCardRow>());

            var reports = _ctx.Employees.Where(p => p.SupervisorId == employeeId).ToList();
            var list = new List<ExecutiveScoreCardRow>();
            foreach (var directReport in reports)
            {
                var data = new RecurseDirectReportData();
                var an = _ctx.Nodes.Active().ProductIsNotExlcuded()
                    .Where(p => p.BuildSpecification.OwnerEmployeeId == directReport.Id);
                var apn = an.InPciScope();
                var all = _ctx.Nodes.Active().ProductIsNotExlcuded()
                    .Where(p => p.OwnerEmployeeId == directReport.Id);

                data.Specs = await _ctx.BuildSpecifications.CountAsync(p => p.OwnerEmployeeId == directReport.Id);
                data.Assigned += new ScoreCardPciCount(await apn.CountAsync(), await an.CountAsync());
                data.Passing += new ScoreCardPciCount(
                    await apn.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope().CountAsync(),
                    await an.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope().CountAsync());
                data.Failing += new ScoreCardPciCount(
                    await apn.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope().CountAsync(),
                    await an.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope().CountAsync());
                data.NotReporting += new ScoreCardPciCount(
                    await apn.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope().CountAsync(),
                    await an.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope().CountAsync());
                data.OutOfChefScope += new ScoreCardPciCount(await apn.OutOfChefScope().CountAsync(),
                    await an.OutOfChefScope().CountAsync());
                data.All += new ScoreCardPciCount(await all.InPciScope().CountAsync(), await all.CountAsync());

                var reportCount = await _ctx.Employees.CountAsync(p => p.SupervisorId == directReport.Id);

                data = await RecurseDirectReports(directReport.Id, data);

                var row = new ExecutiveScoreCardRow(directReport.Id, directReport.OwnerText(), data.Specs,
                    data.Assigned, data.All, data.Passing, data.Failing, data.NotReporting, data.OutOfChefScope,
                    reportCount);
                list.Add(row);
            }

            return new ExecutiveScoreCard(emp.Id, emp.OwnerText(), emp.SupervisorId, emp.Supervisor.OwnerText(),
                list.OrderBy(p => p.Name));
        }

        public async Task<ProductScoreCard> GetProductScoreCard(string productCode)
        {
            var product = await _ctx.Products.FirstOrDefaultAsync(p => p.Code == productCode);
            if (product == null)
                return new ProductScoreCard(productCode, "NA", new ProductScoreCardRow[0]);
            var functions = await _ctx.Nodes.Active().ByProductCode(product.Code)
                .Select(p => new {p.Function.Id, p.Function.Name}).Distinct().ToDictionaryAsync(p => p.Id, p => p.Name);

            var list = new List<ProductScoreCardRow>();
            foreach (var function in functions.Keys)
            {
                var allByFunc = _ctx.Nodes.Active().ByFunction(productCode, function);
                if (await allByFunc.AnyAsync())
                {
                    var owners = await allByFunc.Select(p => p.Owner).Distinct().ToDictionaryAsync(p => p.Id, p => p);
                    foreach (var owner in owners.Keys)
                    {
                        var all = _ctx.Nodes.Active().ByFunction(productCode, function).ByOwner(owner);

                        var an = all.Assigned();
                        var apn = an.InPciScope();
                        var specs = await an.Select(p => p.BuildSpecificationId).Distinct().CountAsync();

                        var assigned = new ScoreCardPciCount(await apn.CountAsync(), await an.CountAsync());
                        var passing = new ScoreCardPciCount(
                            await apn.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope().CountAsync(),
                            await an.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope().CountAsync());
                        var failing = new ScoreCardPciCount(
                            await apn.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope().CountAsync(),
                            await an.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope().CountAsync());
                        var notReporting = new ScoreCardPciCount(
                            await apn.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope()
                                .CountAsync(),
                            await an.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope()
                                .CountAsync());
                        var outOfChefScope = new ScoreCardPciCount(
                            await apn.OutOfChefScope()
                                .CountAsync(),
                            await an.OutOfChefScope()
                                .CountAsync());
                        var allNodes =
                            new ScoreCardPciCount(await all.InPciScope().CountAsync(), await all.CountAsync());

                        var row = new ProductScoreCardRow(function, functions[function], owners[owner].Id,
                            owners[owner].OwnerText(false), owners[owner].ReportingDirectorId,
                            owners[owner].ReportingDirector.OwnerText(false), specs, allNodes, assigned,
                            passing, failing, notReporting, outOfChefScope);
                        list.Add(row);
                    }
                }
            }

            return new ProductScoreCard(product.Code, product.Name, list.ToArray());
        }

        public async Task<DirectorScoreCard> GetDirectorScoreCard()
        {
            var directors = (await _ctx.Nodes.Active().Where(p => p.Owner.ReportingDirectorId != null)
                    .Select(p => p.Owner.ReportingDirector).Distinct()
                    .OrderBy(p => p.PreferredName ?? p.FirstName).ThenBy(p => p.LastName).ToArrayAsync())
                .ToDictionary(p => p.Id, p => p);
            directors.Add(-1,
                new Employee
                {
                    Id = -1,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    PreferredName = "(Not Found)"
                });
            var list = new List<DirectorScoreCardRow>();
            foreach (var director in directors.Keys)
            {
                var owners = await _ctx.Nodes.ByDirector(director == -1 ? null : (int?) director).Select(p => p.Owner)
                    .Distinct().ToDictionaryAsync(p => p.Id, p => p);
                foreach (var owner in owners.Keys)
                {
                    var all = _ctx.Nodes.Active().ByOwner(owner).ProductIsNotExlcuded();
                    if (await all.AnyAsync())
                    {
                        var an = all.Assigned();
                        var apn = an.InPciScope();
                        var specs = await an.Select(p => p.BuildSpecificationId).Distinct().CountAsync();

                        var assigned = new ScoreCardPciCount(await apn.CountAsync(), await an.CountAsync());
                        var passing = new ScoreCardPciCount(
                            await apn.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope().CountAsync(),
                            await an.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope().CountAsync());
                        var failing = new ScoreCardPciCount(
                            await apn.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope().CountAsync(),
                            await an.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope().CountAsync());
                        var notReporting = new ScoreCardPciCount(
                            await apn.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope()
                                .CountAsync(),
                            await an.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope()
                                .CountAsync());
                        var outOfChefScope = new ScoreCardPciCount(
                            await apn.OutOfChefScope()
                                .CountAsync(),
                            await an.OutOfChefScope()
                                .CountAsync());
                        var allNodes =
                            new ScoreCardPciCount(await all.InPciScope().CountAsync(), await all.CountAsync());

                        var row = new DirectorScoreCardRow(owners[owner].Id,
                            owners[owner].OwnerText(false), directors[director].Id,
                            directors[director].OwnerText(false), specs, assigned,
                            passing, failing, notReporting, outOfChefScope, allNodes);
                        list.Add(row);
                    }
                }
            }

            return new DirectorScoreCard(list.ToArray());
        }

        public async Task<string[]> GetProductCodes()
        {
            return await _ctx.Products.Where(p => !p.ExludeFromReports).OrderBy(p => p.Name).Select(p => p.Code)
                .ToArrayAsync();
        }

        public async Task<PlatformScoreCard> GetPlatformScoreCard()
        {
            var allbs = (await _ctx
                    .BuildSpecifications
                    .OfBuildSpecType(BuildSpecificationTypeConstant.OperatingSystem)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Owner,
                        p.OperatingSystemName,
                        p.OperatingSystemVersion,
                        AppSpecCount = p.Children.Count(),
                        NodeCount = p.Children.SelectMany(q => q.Nodes).Count()
                    })
                    .OrderBy(p => p.Name)
                    .ToArrayAsync())
                .Select(p => new PlatformScoreCardRow(p.Id, p.Name, p.Owner.Id, p.Owner.OwnerText(),
                    p.OperatingSystemName, p.OperatingSystemVersion, p.NodeCount, p.AppSpecCount));

            return new PlatformScoreCard(allbs);
        }

        private async Task<RecurseDirectReportData> RecurseDirectReports(long employeeId, RecurseDirectReportData data)
        {
            var direcReportIds = _ctx.Employees.Where(p => p.SupervisorId == employeeId).Select(p => p.Id).ToArray();
            var an = _ctx.Nodes.Active().ProductIsNotExlcuded()
                .Where(p => direcReportIds.Contains(p.BuildSpecification.OwnerEmployeeId));
            var apn = an.InPciScope();
            var all = _ctx.Nodes.Active().ProductIsNotExlcuded()
                .Where(p => direcReportIds.Contains(p.OwnerEmployeeId));

            data.Specs += await _ctx.BuildSpecifications.CountAsync(p => direcReportIds.Contains(p.OwnerEmployeeId));
            data.Assigned += new ScoreCardPciCount(await apn.CountAsync(), await an.CountAsync());
            data.Passing += new ScoreCardPciCount(
                await apn.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope().CountAsync(),
                await an.ByComplianceStatus(ComplianceStatusConstant.Succeeded).InChefScope().CountAsync());
            data.Failing += new ScoreCardPciCount(
                await apn.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope().CountAsync(),
                await an.ByComplianceStatus(ComplianceStatusConstant.Failed).InChefScope().CountAsync());
            data.NotReporting += new ScoreCardPciCount(
                await apn.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope().CountAsync(),
                await an.ByComplianceStatus(ComplianceStatusConstant.NotFound).InChefScope().CountAsync());
            data.OutOfChefScope += new ScoreCardPciCount(await apn.OutOfChefScope().CountAsync(),
                await an.OutOfChefScope().CountAsync());
            data.All += new ScoreCardPciCount(await all.InPciScope().CountAsync(), await all.CountAsync());


            foreach (var direcReportId in direcReportIds) data = await RecurseDirectReports(direcReportId, data);

            return data;
        }

        private class RecurseDirectReportData
        {
            public int Specs { get; set; }
            public ScoreCardPciCount Assigned { get; set; }
            public ScoreCardPciCount All { get; set; }
            public ScoreCardPciCount Passing { get; set; }
            public ScoreCardPciCount Failing { get; set; }
            public ScoreCardPciCount NotReporting { get; set; }
            public ScoreCardPciCount OutOfChefScope { get; set; }
        }
    }
}