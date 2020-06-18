using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Framework;
using act.core.web.Models.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace act.core.web.Services
{
    public interface INodeFactory
    {
        Task<long?> BuildSpecIdByHost(string fqdnOrHostName);
        Task<NodeSearchResult[]> GetAssignedToBuildSpec(long buildSpecId);
        Task AssignBuildSpecification(long nodeId, long? buildSpecId, string userName = null);
        Task<bool> AssignBuildSpecification(string fqdn, long buildSpecId);
        Task<string[]> ChefIdsForAppOrOsSpecAndEnvironment(long specId, int environmentId);
        void AssignLocalTimeOffset(int localTimeOffset);

        Task<NodeSearchResults> Search(PlatformConstant[] platform,
            int[] environment, PciScopeConstant[] pciScope,
            NodeComplianceSearchTypeConstant[] compliance, NodeSearchTypeConstant searchType, string query,
            bool hideProductExclusions, IUserSecurity employeeSecurity, int pageIndex, bool showButtons);

        Task<IDictionary<int ,(string name, string color)>> GetEnvironments();
    }

    internal class NodeFactory : INodeFactory
    {
        private readonly ActDbContext _ctx;
        private readonly ILogger<NodeFactory> _logger;
        private int localTimeOffset;
        public NodeFactory(ActDbContext ctx, ILoggerFactory logger)
        {
            _ctx = ctx;
            _logger = logger.CreateLogger<NodeFactory>();
        }

        public async Task<IDictionary<int ,(string name, string color)>> GetEnvironments()
        {
            var all = await _ctx.Environments.OrderBy(p => p.Id).ToArrayAsync();
            return all.ToDictionary(p => p.Id, p=> (p.Name, p.Color));
        }
        
        public async Task AssignBuildSpecification(long nodeId, long? buildSpecId, string userName = null)
        {
            var it = await _ctx.Nodes.ById(nodeId);
            if (it == null)
                throw new ArgumentException($"A Node with the InventoryItemId {nodeId} was not found.", nameof(nodeId));
            
            var buildSpecExists = buildSpecId.HasValue && await _ctx.BuildSpecifications
                .OfBuildSpecType(BuildSpecificationTypeConstant.Application)
                .ExistsById(buildSpecId.Value);

            if (!buildSpecExists)
            {
                _logger.LogInformation($"Adding BuildSpec as null to {it.Fqdn}/{it.InventoryItemId} by {userName}");
                it.BuildSpecificationId = null;
                await _ctx.SaveChangesAsync();
            }
            else if (it.BuildSpecificationId != buildSpecId.Value)
            {
                it.BuildSpecificationId = buildSpecId.Value;
                await _ctx.SaveChangesAsync();
            }
        }
        public async Task<bool> AssignBuildSpecification(string fqdn, long buildSpecId)
        {
            var it = await _ctx.Nodes.NodeByFqdnOrHostName(fqdn);
            if (it == null)
                throw new ArgumentException($"A Node with the FQDN {fqdn} was not found.", nameof(fqdn));

            var buildSpecExists = await _ctx.BuildSpecifications
                .OfBuildSpecType(BuildSpecificationTypeConstant.Application)
                .ExistsById(buildSpecId);

            if (!buildSpecExists)
                throw new ArgumentException($"An Application Specification with the ID {buildSpecId} was not found.",
                    nameof(buildSpecId));
                
            if (it.BuildSpecificationId != buildSpecId)
            {
                it.BuildSpecificationId = buildSpecId;
                await _ctx.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<string[]> ChefIdsForAppOrOsSpecAndEnvironment(long specId, int environmentId)
        {
            {
                return (await _ctx.Nodes.AsNoTracking().Active()
                    .FindForAppOrOsSpecAndEnvironmentWithComplianceResult(specId, environmentId)
                    .Where(p => p.ChefNodeId != null)
                    .Select(p => p.Fqdn)
                    .ToArrayAsync()).ToArray();
            }
        }

        public async Task<long?> BuildSpecIdByHost(string fqdnOrHostName)
        {
            return await _ctx.Nodes.AsNoTracking().BuildSpecIdByFqdnOrHostName(fqdnOrHostName);            
        }

        public async Task<NodeSearchResult[]> GetAssignedToBuildSpec(long buildSpecId)
        {
            var q = _ctx.Nodes.AsNoTracking().ForBuildSpec(buildSpecId);
            return await ToResult(q, false, localTimeOffset);
        }

        public async Task<NodeSearchResults> Search(PlatformConstant[] platform,
            int[] environment, PciScopeConstant[] pciScope,
            NodeComplianceSearchTypeConstant[] compliance, NodeSearchTypeConstant searchType, string query,
            bool hideProductExclusions, IUserSecurity employeeSecurity, int pageIndex, bool showButtons)
        {
            var valid = !string.IsNullOrWhiteSpace(query);
            const int take = 30;
            var q = _ctx.Nodes.AsNoTracking().Active();
            if (hideProductExclusions) 
                q = q.ProductIsNotExlcuded();

            if (platform?.Length > 0)
            {
                
                q = q.ByPlatforms(platform);

                valid = true;
            }

            if (environment?.Length > 0)
            {
                q = q.ForEnvironments(environment);
                valid = true;
            }

            if (pciScope?.Length > 0)
            {
                q = q.ByPciScopes(pciScope);
                valid = true;
            }

            var complianceFlags = compliance.ConvertToFlag();

            if (complianceFlags.HasValue)
            {
                var cf = complianceFlags.Value;

                var unassigned = cf.HasFlag(NodeComplianceSearchTypeConstant.Unassigned);
                var assigned = cf.HasFlag(NodeComplianceSearchTypeConstant.Assigned);
                var complianceStatuses = new List<ComplianceStatusConstant>();
                
                if (cf.HasFlag(NodeComplianceSearchTypeConstant.NotReporting))
                {
                    complianceStatuses.Add(ComplianceStatusConstant.NotFound);
                }
                if (cf.HasFlag(NodeComplianceSearchTypeConstant.Failing))
                {
                    complianceStatuses.Add(ComplianceStatusConstant.Failed);
                }
                if (cf.HasFlag(NodeComplianceSearchTypeConstant.Passing))
                {
                    complianceStatuses.Add(ComplianceStatusConstant.Succeeded);
                }

                if (complianceStatuses.Any())
                {
                    assigned = true;
                    q = q.ByComplianceStatuses(complianceStatuses.ToArray());
                }

                if (unassigned && !assigned)
                {
                    q = q.Unassigned();
                }
                else if (assigned && !unassigned)
                {
                    q = q.Assigned();
                }

                valid = true;
            }

            switch (searchType)
            {
                case NodeSearchTypeConstant.Mine:
                    if (!string.IsNullOrWhiteSpace(query))
                        q = q.Where(p => EF.Functions.Like(p.Fqdn, $"{query}%"));
                    q = q.Where(p => p.OwnerEmployeeId == employeeSecurity.EmployeeId).OrderBy(p => p.Fqdn);
                    valid = true;
                    break;
                case NodeSearchTypeConstant.Fqdn:
                    if (!string.IsNullOrWhiteSpace(query))
                        q = q.Where(p =>EF.Functions.Like(p.Fqdn, $"{query}%"));

                    q = q.OrderBy(p => p.Fqdn);
                    break;
                case NodeSearchTypeConstant.Owner:
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        var empIds = await q.Select(p => p.Owner).Search(query).Select(p => p.Id).Distinct()
                            .ToArrayAsync();
                        q = q.Where(p => empIds.Contains(p.OwnerEmployeeId));
                    }

                    q = q.OrderBy(p => p.Owner.PreferredName ?? p.Owner.FirstName).ThenBy(p => p.Owner.LastName)
                        .ThenBy(p => p.Fqdn);
                    break;
                case NodeSearchTypeConstant.Director:
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        var empIds =
                            (await q.Where(p => p.Owner.ReportingDirectorId != null)
                                .Select(p => p.Owner.ReportingDirector).Search(query).Select(p => p.Id).Distinct()
                                .ToArrayAsync()).Cast<long?>().ToArray();
                        q = q.Where(p => empIds.Contains(p.Owner.ReportingDirectorId));
                    }

                    q = q.OrderBy(p => p.Owner.PreferredName ?? p.Owner.FirstName).ThenBy(p => p.Owner.LastName)
                        .ThenBy(p => p.Fqdn);
                    break;
                case NodeSearchTypeConstant.Product:
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        var sw = $"{query}%";
                        q = q.Where(p =>
                            EF.Functions.Like(p.Product.Name, sw) || EF.Functions.Like(p.ProductCode, sw) ||
                            EF.Functions.Like(p.Product.Name, sw) || EF.Functions.Like(p.ProductCode, sw) ||
                            EF.Functions.Like(p.Function.Name, sw));
                    }

                    q = q.OrderBy(p => p.Product.Name).ThenBy(p => p.Function.Name).ThenBy(p => p.Fqdn);
                    break;
                case NodeSearchTypeConstant.OsSpec:
                    q = q.Where(p => p.BuildSpecificationId.HasValue);
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        var contains = $"%{query}%";
                        q = q.Where(p => EF.Functions.Like(p.BuildSpecification.Parent.Name, contains));
                    }

                    q = q.OrderBy(p => p.BuildSpecification.Name).ThenBy(p => p.Fqdn);
                    break;                
                case NodeSearchTypeConstant.AppSpec:
                    q = q.Where(p => p.BuildSpecificationId.HasValue);
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        var contains = $"%{query}%";
                        q = q.Where(p => EF.Functions.Like(p.BuildSpecification.Name, contains));
                    }

                    q = q.OrderBy(p => p.BuildSpecification.Name).ThenBy(p => p.Fqdn);
                    break;
                default:
                    throw new ArgumentException(@"NodeSearchType not supported.", nameof(searchType));
            }

            if (!valid)
                q = q.Where(p => 1 == 2);

            var matchCount = await q.CountAsync();
            var skip = take * pageIndex;
            var results = matchCount > 0 ? await ToResult(q.Skip(skip).Take(take), showButtons, localTimeOffset) : new NodeSearchResult[0];
            var displayCount = skip + results.Length;
            return new NodeSearchResults(results, matchCount, displayCount);
        }

        private static async Task<NodeSearchResult[]> ToResult(IQueryable<Node> q, bool showButtons, int localTimeOffset)
        {
            return (await q.AsNoTracking()
                    .Select(p => new
                    {
                        p.InventoryItemId,
                        p.Fqdn,
                        p.Owner,
                        ProductName = p.Product.Name,
                        FunctionName = p.Function.Name,
                        p.PciScope,
                        p.EnvironmentId,
                        EnvName = p.Environment.Name,
                        EnvDesc = p.Environment.Description,
                        EnvColor = p.Environment.Color,
                        p.Platform,
                        p.BuildSpecificationId,
                        BuildSpecificationName = p.BuildSpecification == null ? null : p.BuildSpecification.Name,
                        p.ComplianceStatus,
                        p.LastComplianceResultDate,
                        p.ChefNodeId
                    })
                    .ToArrayAsync()) //YANK FROM DB
                .Select(p => new NodeSearchResult(p.InventoryItemId, p.Fqdn, p.Owner.OwnerText(), p.ProductName,
                    p.FunctionName, p.PciScope, p.EnvironmentId, p.EnvName, p.EnvDesc, p.EnvColor,
                    p.Platform, p.BuildSpecificationId,
                    p.BuildSpecificationName, p.ComplianceStatus, p.LastComplianceResultDate,
                    p.ChefNodeId, showButtons, localTimeOffset))
                .ToArray();
        }
        public void AssignLocalTimeOffset(int localTimeOffset)
        {
            this.localTimeOffset = localTimeOffset;
        }
    }
}