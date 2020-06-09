using System;
using act.core.data;
using act.core.etl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string InventorySystemLink(this IUrlHelper helper, long inventoryItemId)
        {
            using (var scope = helper.ActionContext.HttpContext.RequestServices.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var format = config.GetValue<string>("InventorySystemLinkFormat");
                return string.Format(format, inventoryItemId);
            }
        }

        public static string ChefAutomateComplianceReport(this IUrlHelper helper, int environmentId, Guid id)
        {
            using (var scope = helper.ActionContext.HttpContext.RequestServices.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<ActDbContext>();
                var env = ctx.Environments.ById(environmentId).GetAwaiter().GetResult();
                return $"{env.ChefAutomateUrl}/compliance/reports/nodes/{id}";
                
            }
        }
        
        public static string ChefAutomateConvergeReport(this IUrlHelper helper, int environmentId, Guid id)
        {
            using (var scope = helper.ActionContext.HttpContext.RequestServices.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<ActDbContext>();
                var env = ctx.Environments.ById(environmentId).GetAwaiter().GetResult();
                var node = ctx.Nodes.ById(id).GetAwaiter().GetResult();
                var gather = scope.ServiceProvider.GetRequiredService<IGatherer>();
                var result = gather.PostRequest(environmentId, 1, 1, new[] {node?.Fqdn}).GetAwaiter().GetResult();
                return $"{env.ChefAutomateUrl}/infrastructure/client-runs/{id}/runs/{result?.Nodes?[0].ScanData?.id}";
            }
        }

    }
}