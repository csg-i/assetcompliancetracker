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
        public static string PartialPackagesScreen(this IUrlHelper helper, BuildSpecificationTypeConstant specType, JustificationTypeConstant packageType)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "ForSpec", specType, id = packageType });
        }
        public static string PartialNewPackage(this IUrlHelper helper, BuildSpecificationTypeConstant specType, JustificationTypeConstant packageType, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "New", id = packageType, specId, specType });
        }
        public static string PartialNewBulkPackages(this IUrlHelper helper, BuildSpecificationTypeConstant specType, JustificationTypeConstant packageType, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "NewBulk", id = packageType, specId, specType });
        }
        public static string JsonAddPackage(this IUrlHelper helper, JustificationTypeConstant packageType)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "Add", id = packageType });
        }
        public static string JsonBulkAddPackages(this IUrlHelper helper, JustificationTypeConstant packageType)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "BulkAdd", id = packageType });
        }
        
        public static string JsonCleanupDuplicatePackages(this IUrlHelper helper, JustificationTypeConstant packageType, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "CleanDuplicates", id = packageType, specId });
        }
        public static string JsonAssignPackageJustification(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "Assign", id });
        }
        public static string JsonDeletePackage(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "Delete", id });
        }
        public static string PartialGetPackage(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "Single", id });
        }
        public static string PartialEditPackage(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "Edit", id });
        }
        public static string JsonSavePackage(this IUrlHelper helper, long id)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "Save", id });
        }
        public static string PartialGetPackages(this IUrlHelper helper, JustificationTypeConstant packageType, long specId)
        {
            return helper.RouteUrl("default", new { controller = "Packages", action = "All", id = packageType, specId });
        }
    }
}