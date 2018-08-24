using System;
using act.core.data;
using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string ReportsHome(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Report", action = "Home" });
        }
        
        public static string PortReport(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Report", action = "Ports", id });
        }
        
        public static string AssignedNodesReport(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Report", action = "AssignedNodes" });
        }
        
        public static string NodesExcludedByProductReport(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Report", action = "ExcludedProduct" });
        }
        
        public static string NotReportingNodesReport(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Report", action = "NotReporting" });
        }
        
        public static string AllPortsReport(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Report", action = "AllPorts" });
        }
        
        public static string OwnerReport(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Report", action = "Owners" });
        }

        public static string Review(this IUrlHelper helper, long specId, int? environmentId = null)
        {
            if(environmentId.HasValue)
                return helper.RouteUrl("default", new { controller = "Report", action = "Review", id = specId, environmentId});

            return helper.RouteUrl("default", new { controller = "Report", action = "Review", id = specId });
        }
    }
}