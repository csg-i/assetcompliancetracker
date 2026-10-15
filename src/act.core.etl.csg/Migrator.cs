using act.core.data;
using Microsoft.Extensions.Configuration;
using Amazon.Lambda.Core;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Environment = act.core.data.Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace act.core.etl.csg
{
    //Access ACT Data
    class Migrator : IMigrator
    {
        private readonly ActDbContext _ctx;
        private const int Take = 500;
        private readonly string _gam;
        private readonly string _gemp;
        private readonly ChefAutomate _chefAutomate;

        public Migrator(ActDbContext ctx, IConfiguration configuration)
        {
            _ctx = ctx;
            _gam = configuration.GetConnectionString("GAM");
            _gemp = configuration.GetConnectionString("GEMP");
            var chefautomateConfigurationSections = configuration.GetSection("ChefAutomate").GetChildren().Select(p => p).ToList();
            _chefAutomate = new ChefAutomate
            {
                NonProdURL = chefautomateConfigurationSections.Where(r => r.Key == "NonProdURL").Select(r => r.Value).FirstOrDefault(),
                NonProdToken = chefautomateConfigurationSections.Where(r => r.Key == "NonProdToken").Select(r => r.Value).FirstOrDefault(),
                ProdURL = chefautomateConfigurationSections.Where(r => r.Key == "ProdURL").Select(r => r.Value).FirstOrDefault(),
                ProdToken = chefautomateConfigurationSections.Where(r => r.Key == "ProdToken").Select(r => r.Value).FirstOrDefault()
            };
        }

        private static void AddFromToParameters(long pageIndex, SqlCommand cmd)
        {
            var start = pageIndex * Take;
            var from = cmd.Parameters.Add("@from", SqlDbType.BigInt);
            from.Value = start + 1;
            var to = cmd.Parameters.Add("@to", SqlDbType.BigInt);
            to.Value = start + Take;
        }

        private static async Task<List<T>> Traverse<T>(string db, string sql, long? pageIndex = null) where T : class, new()
        {
            var list = new List<T>();
            using (var cn = new SqlConnection(db))
            {
                await cn.OpenAsync();
                using (var cmd = new SqlCommand(sql, cn))
                {
                    if (pageIndex.HasValue)
                        AddFromToParameters(pageIndex.Value, cmd);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            var fields = Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, i => i);
                            while (await reader.ReadAsync())
                            {
                                list.Add(reader.ReflectType<T>(fields));
                            }
                        }
                    }
                }
            }

            return list;
        }

        private async Task<List<Employee>> TraverseEmployees(long pageIndex)
        {
            return await Traverse<Employee>(_gemp, Sql.EmployeeQuery, pageIndex);
        }

        private async Task<List<Node>> TraverseNodes(long pageIndex)
        {
            return await Traverse<Node>(_gam, Sql.NodeQuery, pageIndex);

        }
        private async Task<List<Function>> TraverseFunctions()
        {
            return await Traverse<Function>(_gam, Sql.FunctionQuery);
        }
        private async Task<List<Product>> TraverseProducts()
        {
            return await Traverse<Product>(_gam, Sql.ProductQuery);
        }
        public async Task<int> AddOrUpdateFunctions()
        {
            var counter = 0;
            var functions = await TraverseFunctions();
            foreach (var function in functions)
            {
                if (await _ctx.Functions.AnyAsync(p => p.Id == function.Id))
                {
                    _ctx.Functions.Update(function);
                }
                else
                {
                    await _ctx.Functions.AddAsync(function);
                }

                counter += 1;
            }

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                LambdaLogger.Log($"DbUpdateException Updating Function Metadata : {ex}");
            }
            catch (SqlException ex)
            {
                LambdaLogger.Log($"SqlException Updating Function Metadata : {ex}");
            }
            catch (MySqlException ex)
            {
                LambdaLogger.Log($"MySqlException Updating Function Metadata : {ex}");
            }

            LambdaLogger.Log($"Updated Function Metadata");

            return counter;
        }

        public async Task<int> AddOrUpdateProducts()
        {
            var counter = 0;
            var products = await TraverseProducts();
            foreach (var product in products)
            {
                var toUpdate = _ctx.Products.AsNoTracking().FirstOrDefault(p => p.Code == product.Code);
                if (toUpdate == null)
                {
                    await _ctx.Products.AddAsync(product);
                }
                else
                {
                    _ctx.Entry(toUpdate).State = EntityState.Detached;
                    product.ExludeFromReports = toUpdate.ExludeFromReports;
                    _ctx.Products.Update(product);
                }

                counter += 1;
            }

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                LambdaLogger.Log($"DbUpdateException Updating Product Metadata : {ex}");
            }
            catch (SqlException ex)
            {
                LambdaLogger.Log($"SqlException Updating Product Metadata : {ex}");
            }
            catch (MySqlException ex)
            {
                LambdaLogger.Log($"MySqlException Updating Product Metadata : {ex}");
            }

            LambdaLogger.Log($"Updated Product Metadata");

            return counter;
        }

        public async Task<int> AddOrUpdateEmployees(int pageIndex)
        {
            var counter = 0;
            var employees = await TraverseEmployees(pageIndex);
            foreach (var employee in employees)
            {
                try
                {
                    if (employee.SupervisorId.HasValue)
                    {
                        if (!await _ctx.Employees.AnyAsync(p => p.Id == employee.SupervisorId))
                        {

                            LambdaLogger.Log($"Attempted to update employee '{employee.Id}-{employee.FirstName} {employee.LastName}' but the supervisor {employee.SupervisorId} was not loaded yet.");

                            employee.SupervisorId = null;
                        }
                    }

                    if (employee.ReportingDirectorId.HasValue)
                    {
                        if (!await _ctx.Employees.AnyAsync(p => p.Id == employee.ReportingDirectorId))
                        {

                            LambdaLogger.Log(
                             $"Attempted to update employee '{employee.Id}-{employee.FirstName} {employee.LastName}' but the director {employee.ReportingDirectorId} was not loaded yet.");


                            employee.ReportingDirectorId = null;
                        }
                    }

                    var toUpdate = await _ctx.Employees.AsNoTracking().ById(employee.Id);
                    if (toUpdate == null)
                    {
                        //Check if SamAccountName is already present, if yes delete and add new record else only add new record
                        var toDelete = await _ctx.Employees.AsNoTracking().BySamAccountName(employee.SamAccountName);

                        if (toDelete != null)
                        {
                            //Delete and Add new record
                            _ctx.Entry(toDelete).State = EntityState.Deleted;
                            _ctx.Employees.Remove(toDelete);
                            _ctx.SaveChanges();
                            await _ctx.Employees.AddAsync(employee);
                        }
                        else
                        {
                            //Add new record
                            await _ctx.Employees.AddAsync(employee);
                        }
                    }
                    else
                    {
                        _ctx.Entry(toUpdate).State = EntityState.Detached;
                        _ctx.Employees.Update(employee);
                    }

                    await _ctx.SaveChangesAsync();

                    counter += 1;
                }
                catch (DbUpdateException ex)
                {
                    LambdaLogger.Log($"DbUpdateException Updating Employee for employee {employee.Id} : {ex}");
                }
                catch (SqlException ex)
                {
                    LambdaLogger.Log($"SqlException Updating Employee for employee {employee.Id} : {ex}");
                }
                catch (MySqlException ex)
                {
                    LambdaLogger.Log($"MySqlException Updating Employee for employee {employee.Id} : {ex}");
                }
            }

            LambdaLogger.Log($"Updated Employee Table for Page Index {pageIndex}");

            return counter;
        }

        public async Task<int> GetNodePages()
        {
            return await GetPages(_gam, Sql.NodeCount);
        }

        private static async Task<int> GetPages(string c, string q)
        {
            using (var cn = new SqlConnection(c))
            {
                await cn.OpenAsync();
                using (var cmd = new SqlCommand(q, cn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                var val = reader[0];
                                if (val == DBNull.Value)

                                    return 0;

                                var count = (int?)Convert.ChangeType(val, TypeCode.Int32) ?? 0;
                                return count / Take + (count % Take > 0 ? 1 : 0);
                            }
                        }
                        return 0;
                    }
                }
            }
        }
        public async Task<int> GetEmployeePages()
        {
            return await GetPages(_gemp, Sql.EmployeeCount);
        }

        public async Task<int> AddorUpdateEnvironments()
        {
            var added = 0;
            var updated = 0;
            var environmentList = CreateEnvironmentList();

            foreach (var environment in environmentList)
            {
                var env = _ctx.Environments.FirstOrDefault(p => p.Id == environment.Id);

                if (env == null)
                {
                    _ctx.Environments.Add(environment);
                    added++;
                }
                else
                {
                    _ctx.Environments.Update(environment);
                    updated++;
                }
            }

            if (added > 0 || updated > 0)
                await _ctx.SaveChangesAsync();

            return (added + updated);
        }

        private List<Environment> CreateEnvironmentList()
        {
            return new List<Environment>
            {
                new Environment
                {
                    Id = 1,
                    Name = "NonProd",
                    Description = "All Non-Production Environments",
                    Color = "#6c757d",
                    ChefAutomateUrl = _chefAutomate.NonProdURL,
                    ChefAutomateToken = _chefAutomate.NonProdToken,
                    ChefAutomateOrg = "csg"
                },
                new Environment
                {
                    Id = 12,
                    Name = "Production",
                    Description = "Production Environment",
                    Color = "#ffc107",
                    ChefAutomateUrl = _chefAutomate.ProdURL,
                    ChefAutomateToken = _chefAutomate.ProdToken,
                    ChefAutomateOrg = "csg"
                },
                new Environment
                {
                    Id = 19,
                    Name = "CTE",
                    Description = "Customer Test Environments",
                    Color = "#007bff",
                    ChefAutomateUrl = _chefAutomate.ProdURL,
                    ChefAutomateToken = _chefAutomate.ProdToken,
                    ChefAutomateOrg = "csg"
                }
            };
        }

        public async Task<int> AddOrUpdateNodes(int pageIndex)
        {
            var addCount = 0;
            var updateCount = 0;
            var errorCount = 0;
            var nodes = await TraverseNodes(pageIndex);
            foreach (var node in nodes)
            {
                try
                {
                    var toUpdate = await _ctx.Nodes.AsNoTracking().ById(node.InventoryItemId);
                    if (toUpdate == null)
                    {
                        LambdaLogger.Log($"Adding Node: {node.Fqdn}/{node.InventoryItemId} with BuildSpecificationId as null");
                        await _ctx.Nodes.AddAsync(node);
                        addCount += 1;
                        LambdaLogger.Log($"Attempting to add node id {node.InventoryItemId}.");
                    }
                    else
                    {
                        LambdaLogger.Log($"Updating Node: {node.Fqdn}/{node.InventoryItemId} with {toUpdate.BuildSpecificationId}.");
                        _ctx.Entry(toUpdate).State = EntityState.Detached;
                        node.BuildSpecificationId = toUpdate.BuildSpecificationId;
                        node.ChefNodeId = toUpdate.ChefNodeId;
                        node.LastComplianceResultId = toUpdate.LastComplianceResultId;
                        node.ComplianceStatus = toUpdate.ComplianceStatus;
                        node.LastComplianceResultDate = toUpdate.LastComplianceResultDate;
                        node.FailingSince = toUpdate.FailingSince;
                        node.LastEmailedOn = toUpdate.LastEmailedOn;
                        node.EscalationMailedOn = toUpdate.EscalationMailedOn;
                        node.EscalationStep = toUpdate.EscalationStep;
                        _ctx.Nodes.Update(node);
                        updateCount += 1;
                        LambdaLogger.Log($"Attempting to update node id {node.InventoryItemId}.");
                    }

                    await _ctx.SaveChangesAsync();

                    LambdaLogger.Log($"Node id {node.InventoryItemId} saved.");
                }
                catch (DbUpdateException ex)
                {
                    LambdaLogger.Log($"DbUpdateException on node id {node.InventoryItemId} : {ex}");
                    errorCount += 1;
                }
                catch (SqlException ex)
                {
                    LambdaLogger.Log($"SqlException on node id {node.InventoryItemId} : {ex}");
                    errorCount += 1;
                }
                catch (MySqlException ex)
                {
                    LambdaLogger.Log($"MySqlException on node id {node.InventoryItemId} : {ex}");
                }
            }

            LambdaLogger.Log($"Nodes Added {addCount} / Nodes Updated {updateCount} / Errors {errorCount}");

            return addCount + updateCount;
        }
    }
    class ChefAutomate
    {
        public string NonProdURL { get; set; }
        public string ProdURL { get; set; }
        public string NonProdToken { get; set; }
        public string ProdToken { get; set; }
    }
}
