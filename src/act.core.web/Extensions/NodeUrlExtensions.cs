using act.core.data;
using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string NodeSearch(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Nodes", action = "Index"});
        }

        public static string PartialNodeSearch(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Nodes", action = "Search" });
        }

        public static string NodesForBuildSpec(this IUrlHelper helper, long buildSpecId)
        {
            return helper.RouteUrl("default", new { controller = "Nodes", action = "ForBuildSpec", id = buildSpecId });
        }

        public static string JsonAssignNode(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Nodes", action = "Assign", id });
        }
        public static string JsonChefUrl(this IUrlHelper helper, int id)
        {
            return helper.RouteUrl("default", new { controller = "Nodes", action = "ChefConvergeReportUrlGet", id });
        }
        public static string JsonBuildSpecByHost(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Nodes", action = "BuildSpecId"});
        }
        
        public static string JsonDataGatherForSpec(this IUrlHelper helper, long specId, int environmentId)
        {
            return helper.RouteUrl("default", new { controller = "Nodes", action = "GatherForSpec", id = specId, environmentId });
        }

        public static string JsonComplianceReport(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Nodes", action = "ComplianceReport", id });
        }
    }
}