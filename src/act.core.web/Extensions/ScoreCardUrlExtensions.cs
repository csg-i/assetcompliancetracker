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
        public static string ScoreCardsHome(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "Home" });
        }

        public static string PlatformScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "Platform" });
        }

        public static string PartialPlatformScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "PlatformData" });
        }
        
        public static string FilePlatformScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "PlatformExport" });
        }

        public static string DirectorScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "Director" });
        }

        public static string PartialDirectorScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "DirectorData" });
        }

        public static string FileDirectorScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "DirectorExport" });
        }
        
        public static string ProductScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "Product" });
        }
        
        public static string PartialProductScoreCard(this IUrlHelper helper, string id)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "ProductData", id });
        }
        
        public static string FileProductScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "ProductExport" });
        }
        
        public static string JsonAllProducts(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "Products" });
        }

        public static string ExecutiveScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "Executive" });
        }

        public static string PartialExecutiveScoreCard(this IUrlHelper helper, long? id = null)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "ExecutiveData", id });
        }
        public static string FileExecutiveScoreCard(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "ExecutiveExport", id });
        }

        public static string OwnerScoreCard(this IUrlHelper helper)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "Owner" });
        }

        public static string PartialOwnerScoreCard(this IUrlHelper helper, long? id = null)
        {
            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "OwnerData", id });
        }

        public static string FileOwnerScoreCard(this IUrlHelper helper, long id)
        {

            return helper.RouteUrl("default", new { controller = "ScoreCard", action = "OwnerExport", id });
        } 
    }
}