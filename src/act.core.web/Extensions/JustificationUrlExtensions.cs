using act.core.data;
using Microsoft.AspNetCore.Mvc;

namespace act.core.web.Extensions
{
    /// <summary>
    /// Build routes via Url Helper so you can easily change them in one place
    /// </summary>
    public static partial class UrlExtensions
    {
        public static string PartialGetJustificationsForSpec(this IUrlHelper helper, JustificationTypeConstant type, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Justifications", action = "ForSpec", id = type, specId});
        }
        public static string PartialEditJustification(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Justifications", action = "Edit", id });
        }
        public static string PartialNewJustification(this IUrlHelper helper, JustificationTypeConstant type, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Justifications", action = "New", id = type, specId });
        }
        public static string PartialGetJustification(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Justifications", action = "Single", id });
        }
        public static string JsonDeleteJustification(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Justifications", action = "Delete", id});
        }
        public static string JsonUpateJustification(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Justifications", action = "Update", id });
        }
        public static string JsonChangeJustificationColor(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Justifications", action = "ChangeColor", id });
        }
        public static string JsonAddJustification(this IUrlHelper helper, JustificationTypeConstant type, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Justifications", action = "Add", id = type, specId });
        }
    }
}