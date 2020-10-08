using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Extensions;
using act.core.web.Models.BuildSpec;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Justification = act.core.web.Models.BuildSpec.Justification;

namespace act.core.web.Services
{
    public interface IBuildSpecificationFactory
    {
        Task<JsonInspecAttributes> InspecForFqdn(string fqdn);
        Task<BuildSpec> BuildSpecification(long specId);
        Task<PortReportItems> PortReport(long specId);
        Task<SpecByOwners> ByOwnersReport();
        Task<PortReportItems> AllPortsReport();
        Task<AssignedNodes> AssignedNodesReport();
        Task<long[]> AssignedNodeIds(long specId, int environmentId);
        Task<ReviewData> ReviewComplianceData(long specId, int environmentId);

        Task<ReviewDataItems> ReviewComplianceDataDetails(long specId, int environmentId,
            JustificationTypeConstant resultType, bool shouldExist, PortTypeConstant? portType, string name);

        Task<ReviewDataItems>
            ReviewComplianceDataDetailsForOsFailures(long specId, int environmentId);

        Task<ReviewDataItems> ReviewComplianceDataDetailsForErrors(long specId, int environmentId,
            string name, string code);

        Task<ReviewErrorDetails> ReviewComplianceDataErrorLongMessages(long specId, int environmentId,
            string name, string code);

        Task<ReportingNodes> NodesExludedByProductReport();
        Task<ReportingNodes> NotReportingNodesReport();
    }

    internal class BuildSpecificationFactory : IBuildSpecificationFactory
    {
        private readonly ActDbContext _ctx;
        private readonly object _lockObject = new object();
        private readonly IMemoryCache _cache;

        public BuildSpecificationFactory(ActDbContext ctx, IMemoryCache memoryCache)
        {
            _ctx = ctx;
            _cache = memoryCache;
        }

        private IQueryable<SoftwareComponent> GetOrCreateSoftwareComponent()
        {
            const string key = "softwareComponent";
            lock (_lockObject)
            {
                if (!_cache.TryGetValue(key, out IQueryable<SoftwareComponent> softwareComponents))
                {
                    softwareComponents = _ctx.SoftwareComponents.AsNoTracking()
                        .Include(a => a.SoftwareComponentEnvironments).AsNoTracking();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.High)
                    .SetSize(1)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

                    _cache.Set(key, softwareComponents, cacheEntryOptions);
                }
                return softwareComponents;
            }
        }

        public async Task<PortReportItems> AllPortsReport()
        {
            var all = (await _ctx.Ports.AsNoTracking().OrderBy(p => p.From).ThenBy(p => p.To)
                    .ThenBy(p => p.BuildSpecification.Name)
                    .ThenBy(p => p.PortType).Select(p =>
                        new
                        {
                            Port = p,
                            p.BuildSpecification.BuildSpecificationType,
                            BuildSpecName = p.BuildSpecification.Name,
                            p.BuildSpecification.Owner,
                            p.Justification.JustificationText
                        })
                    .ToArrayAsync())
                .Select(p =>
                    new PortReportItem(p.Port.ToPortString(), p.Port.IsOutgoing, p.Port.IsExternal, p.Port.PortType,
                        p.Owner.OwnerText(false), p.JustificationText, p.BuildSpecificationType, p.BuildSpecName)
                )
                .ToArray();

            return new PortReportItems(all);
        }

        public async Task<AssignedNodes> AssignedNodesReport()
        {
            var all = (await _ctx.Nodes.AsNoTracking().Assigned()
                    .Select(p => new { p.BuildSpecification.Name, p.Fqdn }).OrderBy(p => p.Name).ThenBy(p => p.Fqdn)
                    .ToArrayAsync())
                .Select(p => new AssignedNode(p.Name, p.Fqdn));
            return new AssignedNodes(all);
        }

        public async Task<ReportingNodes> NodesExludedByProductReport()
        {
            return await ToReportingNode(_ctx.Nodes.AsNoTracking()
                .Active()
                .InChefScope()
                .ProductIsExlcuded()
            );
        }

        public async Task<ReportingNodes> NotReportingNodesReport()
        {
            return await ToReportingNode(_ctx.Nodes.AsNoTracking()
                .Active()
                .ByComplianceStatus(ComplianceStatusConstant.NotFound)
                .InChefScope()
                .ProductIsNotExlcuded()
                .Assigned()
            );
        }

        public async Task<long[]> AssignedNodeIds(long specId, int environmentId)
        {
            return await _ctx.Nodes.AsNoTracking().Active()
                .FindForAppOrOsSpecAndEnvironmentWithComplianceResult(specId, environmentId)
                .Select(p => p.InventoryItemId)
                .ToArrayAsync();
        }

        public async Task<ReviewDataItems> ReviewComplianceDataDetailsForOsFailures(long specId,
            int environmentId)
        {
            var date = DateTime.Today.AddDays(-30);
            var nodes = await _ctx.Nodes.AsNoTracking().Assigned()
                .Where(p => p.BuildSpecification.Id == specId || p.BuildSpecification.ParentId == specId)
                .Active()
                .ForEnvironment(environmentId)
                .Where(p => p.LastComplianceResultId != null && p.LastComplianceResultDate > date)
                .Join(_ctx.ComplianceResults.AsNoTracking(),
                    n => new { n.InventoryItemId, ResultId = (Guid)n.LastComplianceResultId },
                    c => new { c.InventoryItemId, c.ResultId },
                    (n, c) => new { Run = c, Node = n }
                )
                .Where(p => p.Run.OperatingSystemTestPassed == false)
                .Select(p => new
                {
                    p.Node.ChefNodeId,
                    p.Node.Fqdn,
                    p.Node.EnvironmentId,
                    Environment = p.Node.Environment.Name,
                    p.Node.PciScope,
                    Product = p.Node.Product.Name,
                    Function = p.Node.Function.Name,
                    p.Node.Owner,
                    BuildSpec = p.Node.BuildSpecificationId == null ? string.Empty : p.Node.BuildSpecification.Name
                })
                .ToArrayAsync();

            var items = nodes.Select(p => new ReviewDataItem(p.ChefNodeId.GetValueOrDefault(), p.Fqdn,
                p.EnvironmentId, p.Environment,
                p.PciScope, p.Product, p.Function, p.Owner.OwnerText(), p.BuildSpec));

            return ReviewDataItems.ForOsFailures(items);
        }

        public async Task<ReviewErrorDetails> ReviewComplianceDataErrorLongMessages(long specId,
            int environmentId, string name, string code)
        {
            var date = DateTime.Today.AddDays(-30);
            var errors = await _ctx.Nodes.AsNoTracking()
                .Where(p => p.BuildSpecification != null &&
                            (p.BuildSpecification.Id == specId || p.BuildSpecification.ParentId == specId))
                .Active()
                .ForEnvironment(environmentId)
                .Where(p => p.LastComplianceResultId != null && p.LastComplianceResultDate > date)
                .Join(_ctx.ComplianceResults.AsNoTracking(),
                    n => new { n.InventoryItemId, ResultId = (Guid)n.LastComplianceResultId },
                    c => new { c.InventoryItemId, c.ResultId },
                    (n, c) => new { Run = c, Node = n }
                )
                .SelectMany(p => p.Run.Errors.Where(c => c.Name == name && c.Code == code))
                .Select(p => new { p.LongMessage, p.ComplianceResult.Node.Fqdn })
                .ToArrayAsync();

            return new ReviewErrorDetails(name, code, errors.Select(p => $"{p.Fqdn}=>{p.LongMessage}"));
        }

        public async Task<ReviewDataItems> ReviewComplianceDataDetailsForErrors(long specId,
            int environmentId, string name, string code)
        {
            var date = DateTime.Today.AddDays(-30);
            var nodes = await _ctx.Nodes.AsNoTracking()
                .Where(p => p.BuildSpecification != null &&
                            (p.BuildSpecification.Id == specId || p.BuildSpecification.ParentId == specId))
                .Active()
                .ForEnvironment(environmentId)
                .Where(p => p.LastComplianceResultId != null && p.LastComplianceResultDate > date)
                .Join(_ctx.ComplianceResults.AsNoTracking(),
                    n => new { n.InventoryItemId, ResultId = (Guid)n.LastComplianceResultId },
                    c => new { c.InventoryItemId, c.ResultId },
                    (n, c) => new { Run = c, Node = n }
                )
                .Where(p => p.Run.Errors.Any(c => c.Name == name && c.Code == code))
                .Select(p => new
                {
                    p.Node.ChefNodeId,
                    p.Node.Fqdn,
                    p.Node.EnvironmentId,
                    Environment = p.Node.Environment.Name,
                    p.Node.PciScope,
                    Product = p.Node.Product.Name,
                    Function = p.Node.Function.Name,
                    p.Node.Owner,
                    BuildSpec = p.Node.BuildSpecificationId == null ? string.Empty : p.Node.BuildSpecification.Name
                })
                .ToArrayAsync();

            var items = nodes.Select(p => new ReviewDataItem(p.ChefNodeId.GetValueOrDefault(), p.Fqdn,
                p.EnvironmentId, p.Environment,
                p.PciScope, p.Product, p.Function, p.Owner.OwnerText(), p.BuildSpec));

            return ReviewDataItems.ForErrors(items);
        }

        public async Task<ReviewDataItems> ReviewComplianceDataDetails(long specId, int environmentId,
            JustificationTypeConstant resultType, bool shouldExist, PortTypeConstant? portType, string name)
        {
            var date = DateTime.Today.AddDays(-30);
            var nodes = await _ctx.Nodes.AsNoTracking()
                .Where(p => p.BuildSpecification != null &&
                            (p.BuildSpecification.Id == specId || p.BuildSpecification.ParentId == specId))
                .Active()
                .ForEnvironment(environmentId)
                .Where(p => p.LastComplianceResultId != null && p.LastComplianceResultDate > date)
                .Join(_ctx.ComplianceResults.AsNoTracking(),
                    n => new { n.InventoryItemId, ResultId = (Guid)n.LastComplianceResultId },
                    c => new { c.InventoryItemId, c.ResultId },
                    (n, c) => new { Run = c, Node = n }
                )
                .Where(p => p.Run.Tests.Any(c =>
                    c.ResultType == resultType && c.PortType == portType && c.Name == name &&
                    c.ShouldExist == shouldExist))
                .Select(p => new
                {
                    p.Node.ChefNodeId,
                    p.Node.Fqdn,
                    p.Node.EnvironmentId,
                    Environment = p.Node.Environment.Name,
                    p.Node.PciScope,
                    Product = p.Node.Product.Name,
                    Function = p.Node.Function.Name,
                    p.Node.Owner,
                    BuildSpec = p.Node.BuildSpecificationId == null ? string.Empty : p.Node.BuildSpecification.Name
                })
                .ToArrayAsync();

            var items = nodes.Select(p => new ReviewDataItem(p.ChefNodeId.GetValueOrDefault(), p.Fqdn,
                p.EnvironmentId, p.Environment,
                p.PciScope, p.Product, p.Function, p.Owner.OwnerText(), p.BuildSpec));

            return new ReviewDataItems(resultType, shouldExist, portType, name, items);
        }

        public async Task<ReviewData> ReviewComplianceData(long specId, int environmentId)
        {
            var date = DateTime.Today.AddDays(-30);
            var spec = await _ctx.BuildSpecifications.Where(p => p.Id == specId).Select(p => p.Name)
                .FirstOrDefaultAsync();
            var nodePart = _ctx.Nodes.AsNoTracking()
                .Where(p => p.BuildSpecification != null &&
                            (p.BuildSpecification.Id == specId || p.BuildSpecification.ParentId == specId))
                .Active()
                .ForEnvironment(environmentId)
                .Where(p => p.LastComplianceResultId != null && p.LastComplianceResultDate > date)
                .Join(_ctx.ComplianceResults.AsNoTracking(),
                    n => new { n.InventoryItemId, ResultId = (Guid)n.LastComplianceResultId },
                    c => new { c.InventoryItemId, c.ResultId },
                    (n, c) => c
                );

            var osFailures = await nodePart
               .CountAsync(p => !p.OperatingSystemTestPassed);

            var collection = await nodePart
                .Join(
                    _ctx.ComplianceResultTests.AsNoTracking(),
                    c => c.Id,
                    t => t.ComplianceResultId,
                    (c, t) => t
                )
                .GroupBy(t => new { t.ResultType, t.ShouldExist, t.PortType, t.Name })
                .Select(g => new FailedTest
                {
                    ResultType = g.Key.ResultType,
                    ShouldExist = g.Key.ShouldExist,
                    PortType = g.Key.PortType,
                    Name = g.Key.Name,
                    Count = g.Count()
                }).ToArrayAsync();

            var errors = await nodePart
                .Join(
                    _ctx.ComplianceResultErrors.AsNoTracking(),
                    c => c.Id,
                    t => t.ComplianceResultId,
                    (c, t) => t
                )
                .GroupBy(p => new { p.Name, p.Code })
                .Select(g => new Error { Name = g.Key.Name, Code = g.Key.Code, Count = g.Count() })
                .ToArrayAsync();

            var env = await _ctx.Environments.ById(environmentId);
            return new ReviewData(specId, spec, environmentId, env.Name, osFailures, errors, collection);
        }

        public async Task<PortReportItems> PortReport(long specId)
        {
            var type = BuildSpecificationTypeConstant.Application;
            var spec = await _ctx.BuildSpecifications.AsNoTracking().ById(type, specId);

            if (spec == null)
            {
                type = BuildSpecificationTypeConstant.OperatingSystem;
                spec = await _ctx.BuildSpecifications.AsNoTracking().ById(type, specId);
                if (spec == null)
                    throw new ArgumentException(nameof(specId));
            }

            var ids = new HashSet<long> { spec.Id };
            if (spec.ParentId.HasValue) ids.Add(spec.ParentId.Value);


            var all = _ctx.Ports.AsNoTracking().Where(p => ids.Contains(p.BuildSpecificationId))
                .OrderBy(p => p.From)
                .ThenBy(p => p.To)
                .ThenBy(p => p.BuildSpecification.Name)
                .ThenBy(p => p.PortType)
                .Select(p => new PortReportItem(p.ToPortString(), p.IsOutgoing, p.IsExternal, p.PortType,
                    p.BuildSpecification.Owner.OwnerText(false),
                    p.Justification.JustificationText,
                    p.BuildSpecification.BuildSpecificationType,
                    p.BuildSpecification.Name))
                .ToArray();

            return new PortReportItems(all);
        }

        public async Task<SpecByOwners> ByOwnersReport()
        {
            var results = await _ctx
                .BuildSpecifications.AsNoTracking()
                .GroupBy(b => new
                {
                    b.Owner.Id,
                    b.Owner.FirstName,
                    b.Owner.LastName,
                    b.Owner.PreferredName,
                    b.Owner.SamAccountName,
                    b.BuildSpecificationType
                })
                .Select(g => new
                {
                    g.Key.Id,
                    Owner = string.IsNullOrWhiteSpace(g.Key.PreferredName) ? g.Key.FirstName + g.Key.LastName : g.Key.PreferredName,
                    g.Key.SamAccountName,
                    g.Key.BuildSpecificationType,
                    Count = g.Count()
                })
                .ToArrayAsync();


            var dict = new Dictionary<long, SpecByOwner>();
            foreach (var r in results)
            {
                if (!dict.ContainsKey(r.Id))
                    dict.Add(r.Id, new SpecByOwner(r.Owner, r.SamAccountName));
                dict[r.Id].SetCount(r.BuildSpecificationType, r.Count);
            }

            return new SpecByOwners(dict.Values.OrderBy(p => p.Owner).ToArray());
        }

        public async Task<BuildSpec> BuildSpecification(long specId)
        {
            var type = BuildSpecificationTypeConstant.Application;
            var appSpec = await _ctx.BuildSpecifications.AsNoTracking()
                .Include(p => p.Parent)
                .Include(p => p.Nodes)
                .ById(type, specId);
            var softwareComponent = GetOrCreateSoftwareComponent();
            BuildSpecification osSpec;
            if (appSpec == null)
            {
                type = BuildSpecificationTypeConstant.OperatingSystem;
                osSpec = await _ctx.BuildSpecifications.AsNoTracking()
                    .Include(p => p.SoftwareComponents)
                    .ById(type, specId);
                if (osSpec == null)
                    throw new ArgumentException(nameof(specId));

                appSpec = new BuildSpecification
                {
                    Name = osSpec.Name,
                    Owner = osSpec.Owner,
                    Ports = new List<Port>(0),
                    SoftwareComponents = new List<SoftwareComponent>(0),
                    Nodes = new List<Node>(0)
                };
            }
            else
            {
                
                appSpec.SoftwareComponents =
                    softwareComponent.Where(x => x.BuildSpecificationId == appSpec.Id).ToList();

                appSpec.Parent.SoftwareComponents =
                    softwareComponent.Where(x => x.BuildSpecificationId == appSpec.Parent.Id).ToList();
                osSpec = appSpec.Parent;
            }

            var apps = appSpec.RunningCoreOs
                ? osSpec.SoftwareComponents.Where(p => !p.NonCore).ToList()
                : osSpec.SoftwareComponents.ToList();

            apps.AddRange(appSpec.SoftwareComponents);

            var ids = apps.Select(p => p.JustificationId).Where(p => p.HasValue).Select(p => p.Value).Distinct()
                .ToArray();


            var sofwareJts = new List<JustificationTypeConstant>();
            switch (osSpec.Platform.GetValueOrDefault(PlatformConstant.Other))
            {
                case PlatformConstant.Unix:
                case PlatformConstant.Linux:
                    sofwareJts.Add(JustificationTypeConstant.Package);
                    break;
                case PlatformConstant.WindowsServer:
                case PlatformConstant.WindowsClient:
                    sofwareJts.Add(JustificationTypeConstant.Application);
                    sofwareJts.Add(JustificationTypeConstant.Feature);
                    break;
            }

            var justifications = (await _ctx
                    .Justifications
                    .Where(p => ids.Contains(p.Id))
                    .Where(p => sofwareJts.Contains(p.JustificationType))
                    .Select(p => new
                    {
                        p.Id,
                        p.JustificationText,
                        p.JustificationType,
                        p.BuildSpecification.Owner,
                        p.BuildSpecification.BuildSpecificationType
                    })
                    .ToArrayAsync()) //Yank from DB
                .Select(j => new Justification(j.JustificationType, j.JustificationText, j.Owner.OwnerText(false),
                    j.BuildSpecificationType, apps.Where(p => p.JustificationId == j.Id)
                        .OrderBy(p => p.JustificationType)
                        .Select(p => new Software(p.Name, p.Description, p.JustificationType, p.NonCore, p.PciScope,
                            p.SoftwareComponentEnvironments.GetEnvironmentNames()))))
                .ToArray();

            var uList = apps.Where(p => p.JustificationId == null && sofwareJts.Contains(p.JustificationType))
                .OrderBy(p => p.JustificationType)
                .Select(p => new Software(p.Name, p.Description, p.JustificationType, p.NonCore, p.PciScope,
                    p.SoftwareComponentEnvironments.GetEnvironmentNames()))
                .ToArray();
            var nodes = appSpec.Nodes.Select(p => new InventorySystemNode(p.InventoryItemId, p.Fqdn)).ToArray();

            return new BuildSpec(specId, type, appSpec.Name, appSpec.Owner.OwnerText(),
                osSpec.OperatingSystemName, osSpec.OperatingSystemVersion, appSpec.WikiLink, appSpec.Overview,
                justifications, await PortReport(specId), uList, nodes);
        }


        public async Task<JsonInspecAttributes> InspecForFqdn(string fqdn)
        {
            var node = await _ctx.Nodes.AsNoTracking()
                .Include(p => p.BuildSpecification)
                .Include(p => p.BuildSpecification.Parent)
                .Include(p => p.BuildSpecification.Parent.Ports)
                .Include(p => p.BuildSpecification.Ports)
                .Active().AsNoTracking().NodeByFqdnOrHostName(fqdn);

            if (node?.BuildSpecificationId == null)
                return JsonInspecAttributes.Empty(fqdn);

            var softwareComponent = GetOrCreateSoftwareComponent();
            node.BuildSpecification.SoftwareComponents =
                softwareComponent.Where(x => x.BuildSpecificationId == node.BuildSpecification.Id).ToList();

            node.BuildSpecification.Parent.SoftwareComponents =
                softwareComponent.Where(x => x.BuildSpecificationId == node.BuildSpecification.Parent.Id).ToList();
            var pt = node.Platform;
            var spec = node.BuildSpecification;

            RecurseSpecs(spec, spec.RunningCoreOs, node.PciScope, node.EnvironmentId, out var portList,
                out var software,
                out var osName, out var osVersion, ref pt);
            var ports = BreakUpPorts(portList);
            switch (pt)
            {
                case PlatformConstant.Other:
                case PlatformConstant.Appliance:
                    return JsonInspecAttributes.ForOther(fqdn, osName, osVersion,
                        PortsOfType(ports, PortTypeConstant.Tcp),
                        PortsOfType(ports, PortTypeConstant.Udp));
                case PlatformConstant.Linux:
                case PlatformConstant.Unix:
                    return JsonInspecAttributes.ForNix(fqdn, osName, osVersion,
                        PortsOfType(ports, PortTypeConstant.Tcp),
                        PortsOfType(ports, PortTypeConstant.Udp),
                        PortsOfType(ports, PortTypeConstant.Tcp6),
                        PortsOfType(ports, PortTypeConstant.Udp6),
                        SoftwareComponentsOfType(software, JustificationTypeConstant.Package));
                case PlatformConstant.WindowsClient:
                case PlatformConstant.WindowsServer:
                    return JsonInspecAttributes.ForWindows(fqdn, osName, osVersion,
                        PortsOfType(ports, PortTypeConstant.Tcp),
                        PortsOfType(ports, PortTypeConstant.Udp),
                        SoftwareComponentsOfType(software, JustificationTypeConstant.Feature),
                        SoftwareComponentsOfType(software, JustificationTypeConstant.Application));
                default:
                    return JsonInspecAttributes.Empty(fqdn);
            }
        }

        private async Task<ReportingNodes> ToReportingNode(IQueryable<Node> nodes)
        {
            var all = (await nodes.AsNoTracking()
                    .Select(p => new { p.Owner, p.Owner.ReportingDirector, p.Fqdn, p.PciScope })
                    .OrderBy(p => p.Owner.PreferredName ?? p.Owner.FirstName).ThenBy(p => p.Owner.LastName)
                    .ThenBy(p => p.Fqdn)
                    .ToArrayAsync())
                .Select(p => new ReportingNode(p.ReportingDirector.OwnerText(false), p.Owner.OwnerText(false), p.Fqdn,
                    p.PciScope));
            return new ReportingNodes(all);
        }

        private static void RecurseSpecs(BuildSpecification spec, bool runningCoreOs, PciScopeConstant pciScope,
            int environmentId, out List<Port> ports, out List<SoftwareComponent> softwares,
            out string osName, out string osVersion, ref PlatformConstant pt)
        {
            softwares = new List<SoftwareComponent>();
            ports = new List<Port>();
            osName = spec.OperatingSystemName;
            osVersion = spec.OperatingSystemVersion;
            var it = spec;
            while (it != null)
            {
                osName = it.OperatingSystemName;
                osVersion = it.OperatingSystemVersion;
                var softToAdd = runningCoreOs
                    ? it.SoftwareComponents.Where(p => !p.NonCore)
                    : it.SoftwareComponents;
                softToAdd = softToAdd.Where(p => p.PciScope == null || (p.PciScope & pciScope) != 0);
                softToAdd = softToAdd.Where(p => !p.SoftwareComponentEnvironments.Any() || p.SoftwareComponentEnvironments.Any(q => q.EnvironmentId == environmentId));
                softwares.AddRange(softToAdd);
                ports.AddRange(it.Ports.Where(p => !p.IsOutgoing));
                if (it.Platform.HasValue)
                    pt = it.Platform.Value;
                it = it.Parent;
            }
        }

        private static string[] SoftwareComponentsOfType(IEnumerable<SoftwareComponent> software,
            JustificationTypeConstant type)
        {
            return software.Where(p => p.JustificationType == type).Select(p => p.Name).Distinct()
                .ToArray();
        }

        private static int[] PortsOfType(IDictionary<PortTypeConstant, int[]> ports, PortTypeConstant type)
        {
            int[] ret;
            if (ports.TryGetValue(type, out ret))
                return ret;

            return new int[0];
        }

        private static IDictionary<PortTypeConstant, int[]> BreakUpPorts(ICollection<Port> ports)
        {
            var dict = new Dictionary<PortTypeConstant, int[]>();
            var types = ports.Select(p => p.PortType).Distinct();
            foreach (var type in types)
            {
                var list = new List<int>();
                var ofType = ports.Where(p => p.PortType == type).ToArray();
                foreach (var port in ofType)
                    if (port.To.HasValue && port.To.Value > port.From)
                        list.AddRange(Enumerable.Range(port.From, port.To.Value - port.From + 1));
                    else
                        list.Add(port.From);
                dict.Add(type, list.Distinct().ToArray());
            }

            return dict;
        }
    }
}