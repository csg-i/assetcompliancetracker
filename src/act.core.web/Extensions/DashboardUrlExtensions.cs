using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string JsonDashboardSpread(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Dashboard", action = "Spread" });
        }
        public static string JsonDashboardDirectorChart(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Dashboard", action = "DirectorChart" });
        }
        public static string JsonDashboardStatus(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Dashboard", action = "Status" });
        }
        public static string JsonDashboardTopOffenders(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Dashboard", action = "TopOffenders" });
        }

        public static string JsonDashboardComplianceOverTime(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Dashboard", action = "ComplianceOverTime" });
        }
    }
}