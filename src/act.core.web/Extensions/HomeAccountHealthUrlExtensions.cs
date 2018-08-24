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


        public static string Home(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new {controller = "Home", action = "Index"});
        }
        public static string Logon(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Account", action = "SignIn" });
        }

        public static string Logoff(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "Account", action = "SignOut" });
        }
        
        public static string Health(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new {controller = "Health", action = "Index"});
        }
        
        public static string HelpHome(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new {controller = "Home", action = "Help"});
        }
    }
}