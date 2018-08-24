using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {

        public static string AppSpecs(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "Index" });
        }

        public static string AppSpecsWizard(this IUrlHelper helper, long? id = null)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "Wizard", id });
        }
        public static string JsonCloneAppSpec(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "Clone", id });
        }

        internal static string AppSpecsWizardFromClone(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "Wizard", id, fromClone = true });
        }

        public static string PartialAppSpecSearch(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "Search" });
        }

        public static string JsonAppSpecTypeAheadSearch(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "TypeAheadSearch" });
        }

        public static string JsonSaveAppSpecInfo(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "Save" });
        }
        public static string JsonDeleteAppSpec(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "Delete", id });
        }
        public static string PartialAppSpecInformation(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "AppSpecs", action = "Information" });
        }
    }
}