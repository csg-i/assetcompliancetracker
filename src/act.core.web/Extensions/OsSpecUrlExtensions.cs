using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string OsSpecs(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "Index" });
        }

        public static string OsSpecsWizard(this IUrlHelper helper, long? id = null)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "Wizard", id });
        }
        public static string JsonCloneOsSpec(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "Clone", id });
        }

        internal static string OsSpecsWizardFromClone(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "Wizard", id, fromClone=true });
        }

        public static string PartialOsSpecSearch(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "Search"});
        }
        public static string JsonOsSpecTypeAheadSearch(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "TypeAheadSearch" });
        }
        
        public static string JsonSaveOsSpecInfo(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "Save" });
        }

        public static string JsonDeleteOsSpec(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "Delete", id });
        }

        public static string PartialOsSpecInformation(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "OsSpecs", action = "Information" });
        }
    }
}