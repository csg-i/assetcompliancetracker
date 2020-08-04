using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace act.core.data
{
    public static class ContextExtensions
    {
        private enum EnvironmentList
        {
            NonProd = 1,
            Production = 12,
            CTE = 19
        }
        public static T? ConvertToFlag<T>(this T[] flags) where T : struct, IConvertible
        {
            if (flags == null || flags.Length == 0)
                return null;
                
            if (!typeof(T).IsEnum)
                throw new NotSupportedException($"{typeof(T)} must be an enumerated type.");

            return (T)(object)flags.Cast<int>().Aggregate(0, (c, n) => c |= n);
        }
       
        public static T? GetNullableFieldValue<T>(this DbDataReader r, int ordinal) where T : struct
        {
            var val = r.GetValue(ordinal);
            if (val == DBNull.Value)
                return null;
            return (T)val;
        }
        public static string GetNullableString(this DbDataReader r, int ordinal)
        {
            var val = r.GetValue(ordinal);
            if (val == DBNull.Value)
                return null;
            return ((string)val).TrimEnd();
        }

        public static async Task<T[]> ExecuteReaderAnync<T>(this DbContext ctx, string sql, Func<DbDataReader, T> mapper, params DbParameter[] parms)
        {
            using (var cmd = ctx.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = sql;
                if (parms != null && parms.Length > 0)
                    cmd.Parameters.AddRange(parms);

                await ctx.Database.OpenConnectionAsync();

                var list = new List<T>();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(mapper.Invoke(reader));
                    }
                }

                return list.ToArray();
            }
        }
        
        public static async Task<int> ExecuteCommandAsync(this DbContext ctx, string sql, params DbParameter[] parms)
        {
            using (var cmd = ctx.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = sql;
                if (parms != null && parms.Length > 0)
                    cmd.Parameters.AddRange(parms);

                await ctx.Database.OpenConnectionAsync();

                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public static string OwnerText(this Employee employee, bool includeSam = true)
        {
            if (employee == null)
                return string.Empty;

            var pn = employee.PreferredName;
            if (string.IsNullOrWhiteSpace(pn)) pn = $"{employee.FirstName} {employee.LastName}";
            if (includeSam)
                return $"{pn} ({employee.SamAccountName.Trim()})";

            return pn;
        }

        public static async Task<bool> Exists(this IQueryable<ComplianceResult> queryable, long inventoryItemId,
            Guid runId)
        {
            return await queryable.AnyAsync(p => p.ResultId == runId && p.InventoryItemId == inventoryItemId);
        }

        public static async Task<T> ById<T>(this IQueryable<T> queryable, long id) where T : LongId
        {
            return await queryable.FirstOrDefaultAsync(p => p.Id == id);
        }

        public static async Task<Employee> ById(this IQueryable<Employee> queryable, long id)
        {
            return await queryable.FirstOrDefaultAsync(p => p.Id == id);
        }

        public static async Task<bool> ExistsById<T>(this IQueryable<T> queryable, long id) where T : LongId
        {
            return await queryable.AnyAsync(p => p.Id == id);
        }

        public static IQueryable<BuildSpecification> OfBuildSpecType(this IQueryable<BuildSpecification> specs,
            BuildSpecificationTypeConstant type)
        {
            var @return = specs.Where(p => p.BuildSpecificationType == type);
            if (type == BuildSpecificationTypeConstant.Application)
                @return = specs.Where(p => p.ParentId != null);

            return @return;
        }
        
        public static async Task<BuildSpecification> ById(this IQueryable<BuildSpecification> specs,
            BuildSpecificationTypeConstant type, long id)
        {
            return await specs.OfBuildSpecType(type).ById(id);
        }

        public static async Task<bool> IsUnique(this IQueryable<BuildSpecification> specs, long? id, string name)
        {
            return !await specs.AnyAsync(p => p.Name == name && p.Id != id);
        }

        public static IQueryable<T> ForBuildSpec<T>(this IQueryable<T> queryable, long id) where T : BuildSpecReference
        {
            return queryable.Where(p => p.BuildSpecificationId == id);
        }

        public static IQueryable<T> OfJustificationType<T>(this IQueryable<T> querable, JustificationTypeConstant type)
            where T : JustificationTypeReference
        {
            return querable.Where(p => p.JustificationType == type);
        }

        public static IQueryable<Port> Justified(this IQueryable<Port> ports)
        {
            return ports.Where(p => p.JustificationId != null);
        }

        public static IQueryable<SoftwareComponent> Unjustified(this IQueryable<SoftwareComponent> sw)
        {
            return sw.Where(p => p.JustificationId == null);
        }

        public static IQueryable<Port> ForJustification(this IQueryable<Port> ports, long justificationId)
        {
            return ports.Where(p => p.JustificationId == justificationId);
        }

        public static async Task<Environment> ById(this IQueryable<Environment> nodes, int environmentId)
        {
            return await nodes.FirstOrDefaultAsync(p => p.Id == environmentId);
        }
        
        public static async Task<Node> ById(this IQueryable<Node> nodes, long inventoryItemId)
        {
            return await nodes.FirstOrDefaultAsync(p => p.InventoryItemId == inventoryItemId);
        }

        public static async Task<Node> ById(this IQueryable<Node> nodes, Guid chefNodeId)
        {
            return await nodes.FirstOrDefaultAsync(p => p.ChefNodeId == chefNodeId);
        }

        public static IQueryable<Node> ByFunction(this IQueryable<Node> nodes, string productCode, int functionId)
        {
            return nodes.Where(p => p.ProductCode == productCode && p.FunctionId == functionId);
        }

        public static IQueryable<Node> ByOwner(this IQueryable<Node> nodes, long id)
        {
            return nodes.Where(p => p.OwnerEmployeeId == id);
        }

        public static IQueryable<Node> ByDirector(this IQueryable<Node> nodes, long? id)
        {
            return nodes.Where(p => p.Owner.ReportingDirectorId == id);
        }

        public static IQueryable<Node> Active(this IQueryable<Node> nodes)
        {
            return nodes.Where(p => p.IsActive);
        }
        
        public static IQueryable<Node> ForEnvironment(this IQueryable<Node> nodes, int environmentId)
        {
            return nodes.Where(p => p.EnvironmentId == environmentId);
        }
        public static IQueryable<Node> ForEnvironments(this IQueryable<Node> nodes, int[] environmentIds)
        {
            return nodes.Where(p => environmentIds.Contains(p.EnvironmentId));
        }

        public static IQueryable<Node> Inactive(this IQueryable<Node> nodes)
        {
            return nodes.Where(p => !p.IsActive);
        }

        public static IQueryable<Node> InPciScope(this IQueryable<Node> nodes)
        {
            return nodes.Where(p => p.PciScope != PciScopeConstant.C);
        }
        
        public static IQueryable<Node> ByPciScope(this IQueryable<Node> nodes, PciScopeConstant pciScope)
        {
            return nodes.Where(p => p.PciScope == pciScope);
        }
        
        public static IQueryable<Node> ByPciScopes(this IQueryable<Node> nodes, PciScopeConstant[] pciScopes)
        {
            return nodes.Where(p => pciScopes.Contains(p.PciScope));
        }

        public static IQueryable<Node> ByPlatform(this IQueryable<Node> nodes, PlatformConstant platform)
        {
            return nodes.Where(p => p.Platform == platform);
        }
        public static IQueryable<Node> ByPlatforms(this IQueryable<Node> nodes, PlatformConstant[] platforms)
        {
            return nodes.Where(p => platforms.Contains(p.Platform));
        }
        
        public static IQueryable<Node> ByComplianceStatus(this IQueryable<Node> nodes, ComplianceStatusConstant status)
        {
            return nodes.Where(p => p.ComplianceStatus == status);
        }
        
        public static IQueryable<Node> ByComplianceStatuses(this IQueryable<Node> nodes, ComplianceStatusConstant[] statuses)
        {
            return nodes.Where(p => statuses.Contains(p.ComplianceStatus));
        }
        
        public static IQueryable<Node> ByProductCode(this IQueryable<Node> nodes, string productCode)
        {
            return nodes.Where(p => p.ProductCode == productCode);
        }
        
        public static IQueryable<Node> ProductIsNotExlcuded(this IQueryable<Node> nodes)
        {
            return nodes.Where(p => !p.Product.ExludeFromReports);
        }
        public static IQueryable<Node> ProductIsExlcuded(this IQueryable<Node> nodes)
        {
            return nodes.Where(p => p.Product.ExludeFromReports);
        }
        
        public static IQueryable<Node> Assigned(this IQueryable<Node> nodes)
        {
            return nodes.Where(p => p.BuildSpecificationId != null);
        }
        
        public static IQueryable<Node> Unassigned(this IQueryable<Node> nodes)
        {
            return nodes.Where(p => p.BuildSpecificationId == null);
        }
        

        public static IQueryable<Node> InChefScope(this IQueryable<Node> nodes)
        {
            return nodes.Where(p => p.Platform == PlatformConstant.Linux || p.Platform == PlatformConstant.WindowsServer || p.Platform == PlatformConstant.WindowsClient);
        }
        
        public static IQueryable<ComplianceResult> InChefScope(this IQueryable<ComplianceResult> results)
        {
            return results.Where(r => r.Node.Platform == PlatformConstant.Linux || r.Node.Platform == PlatformConstant.WindowsServer || r.Node.Platform == PlatformConstant.WindowsClient);

        }
        public static IQueryable<ComplianceResult> Active(this IQueryable<ComplianceResult> results)
        {
            return results.Where(r => r.Node.IsActive);

        }
        
        public static IQueryable<ComplianceResult> ProductIsNotExlcuded(this IQueryable<ComplianceResult> nodes)
        {
            return nodes.Where(p => !p.Node.Product.ExludeFromReports);
        }
        
        public static IQueryable<ComplianceResult> InPciScope(this IQueryable<ComplianceResult> results)
        {
            return results.Where(r => r.Node.PciScope != PciScopeConstant.C);
        }
        
        
        public static IQueryable<ComplianceResultTest> InChefScope(this IQueryable<ComplianceResultTest> results)
        {
            return results.Where(r => r.ComplianceResult.Node.Platform == PlatformConstant.Linux || r.ComplianceResult.Node.Platform == PlatformConstant.WindowsServer || r.ComplianceResult.Node.Platform == PlatformConstant.WindowsClient);

        }
        public static IQueryable<ComplianceResultTest> Active(this IQueryable<ComplianceResultTest> results)
        {
            return results.Where(r => r.ComplianceResult.Node.IsActive);

        }
        
        public static IQueryable<ComplianceResultTest> ProductIsNotExlcuded(this IQueryable<ComplianceResultTest> nodes)
        {
            return nodes.Where(p => !p.ComplianceResult.Node.Product.ExludeFromReports);
        }
        
        public static IQueryable<ComplianceResultTest> InPciScope(this IQueryable<ComplianceResultTest> results)
        {
            return results.Where(r => r.ComplianceResult.Node.PciScope != PciScopeConstant.C);
        }


        public static IQueryable<Node> OutOfChefScope(this IQueryable<Node> nodes)
        {
            return nodes.Where(p =>  p.Platform != PlatformConstant.Linux && p.Platform != PlatformConstant.WindowsServer);
        }

        public static IQueryable<Node> FindForAppOrOsSpecAndEnvironmentWithComplianceResult(this IQueryable<Node> nodes,
            long specId, int environmentId)
        {
            var date = DateTime.Today.AddDays(-30);
            return nodes
                .Where(p => p.BuildSpecification != null &&
                            (p.BuildSpecification.Id == specId || p.BuildSpecification.ParentId == specId))
                .Where(p => p.EnvironmentId == environmentId)
                .Where(p => p.LastComplianceResultId != null && p.LastComplianceResultDate > date);
        }

        public static IQueryable<Node> WithStaleComplianceStatus(this IQueryable<Node> nodes)
        {
            var date = DateTime.Now.AddHours(-48);
            return nodes.Where(p =>
                p.ComplianceStatus != ComplianceStatusConstant.NotFound && p.LastComplianceResultDate < date);
        }

        public static IQueryable<Node> ForBuildSpec(this IQueryable<Node> nodes, long buildSpecId)
        {
            return nodes.Where(p => p.BuildSpecificationId == buildSpecId);
        }

        public static async Task<long?> BuildSpecIdByFqdnOrHostName(this IQueryable<Node> nodes, string fqdnOrHostName)
        {
            if (fqdnOrHostName == null)
                return null;
            
            var hostName = $"{fqdnOrHostName}.%";            
            return await nodes.Where(p => p.Fqdn == fqdnOrHostName || EF.Functions.Like(p.Fqdn, hostName))
                .Assigned().Select(p => p.BuildSpecificationId).FirstOrDefaultAsync();
        }

        public static IQueryable<Employee> Search(this IQueryable<Employee> employees, string query)
        {
            if (query == null)
                return employees.Where(p=>1==0);

            var sw = $"{query}%";


            return employees
                .Where(p => p.IsActive)
                .Where(p => EF.Functions.Like(p.FirstName, sw) ||
                            EF.Functions.Like(p.LastName, sw) ||
                            EF.Functions.Like(p.SamAccountName, sw) ||
                            EF.Functions.Like(p.FirstName + " " + p.LastName, sw) ||
                            EF.Functions.Like(p.LastName + " " + p.FirstName, sw) ||
                            EF.Functions.Like(p.PreferredName, sw)
                );
        }

        public static async Task<Node> NodeByFqdnOrHostName(this IQueryable<Node> nodes,
            string fqdnOrHostName)
        {
            if (fqdnOrHostName == null)
                return null;
            
            var hostName = $"{fqdnOrHostName}.%";            
            return await nodes.Where(p => p.Fqdn == fqdnOrHostName || EF.Functions.Like(p.Fqdn, hostName))
                .FirstOrDefaultAsync();
        }
        
        public static IQueryable<SoftwareComponentEnvironment> BySoftwareComponent(this IQueryable<SoftwareComponentEnvironment> scs, long id)
        {
            return scs.Where(p => p.SoftwareComponentId == id);
        }
        
        public static string GetEnvironmentNames(this IEnumerable<SoftwareComponentEnvironment> scs)
        {
            foreach (var component in scs)
            {
                    component.Environment = new Environment
                    {
                        Name = ((EnvironmentList) component.EnvironmentId).ToString()
                    };
            }
            return string.Join("/", scs.OrderBy(p => p.Environment.Name).Select(p => p.Environment.Name).Distinct());
        }
        
        public static IQueryable<SoftwareComponentEnvironment> ByEnvironment(this IQueryable<SoftwareComponentEnvironment> scs, int id)
        {
            return scs.Where(p => p.EnvironmentId == id);
        }

    }
}
