using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string JsonEmployeeTypeAhead(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Employees", action = "Search" });
        }
    }
}