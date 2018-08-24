using act.core.data;
using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string PartialPortsScreen(this IUrlHelper helper, PlatformConstant platform)
        {
            return helper.RouteUrl("default", new {controller = "Ports", action = "ForSpec", id = platform});
        }
        
        public static string PartialGetPorts(this IUrlHelper helper, PlatformConstant platform,  long specId)
        {
            return helper.RouteUrl("default", new {controller = "Ports", action = "Get", id = platform, specId});
        }

        public static string PartialEditPort(this IUrlHelper helper, PlatformConstant platform, long specId, long justificationId)
        {
            return helper.RouteUrl("default", new { controller = "Ports", action = "Edit", id = platform, specId, justificationId });
        }

        public static string PartialNewPort(this IUrlHelper helper, PlatformConstant platform, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Ports", action = "New", id = platform, specId});
        }
        
        public static string PartialBulkNewPorts(this IUrlHelper helper, PlatformConstant platform, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Ports", action = "Bulk", id = platform, specId });
        }

        public static string JsonValidatePort(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Ports", action = "Valdate"});
        }
        
        public static string JsonSavePort(this IUrlHelper helper, PlatformConstant platform)
        {
            return helper.RouteUrl("default", new { controller = "Ports", action = "Save", id = platform });
        }
        
        public static string PartialOnePort(this IUrlHelper helper, PlatformConstant platform, long specId, long justificationId)
        {
            return helper.RouteUrl("default", new { controller = "Ports", action = "One", id= platform, specId, justificationId});
        }

        public static string JsonDeletePort(this IUrlHelper helper, long justificationId)
        {
            return helper.RouteUrl("default", new { controller = "Ports", action = "Delete", id = justificationId });
        }
    }
}