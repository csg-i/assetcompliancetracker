using System;
using act.core.data;
using act.core.web.Models.OsSpecs;
using act.core.web.Services;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class OsSpecsController : SpecsControllerBase<OsSpecInformation, OsSpecSearchResult, OsSpecSearchResults>
    {
        public OsSpecsController(ISpecificationFactory<OsSpecInformation, OsSpecSearchResult> specificationFactory, ILoggerFactory logger) : base(specificationFactory, logger)
        {
        }

        protected override BuildSpecificationTypeConstant BuildSpecificationType => BuildSpecificationTypeConstant.OperatingSystem;
        protected override Action<long> LogDelete => (id) =>
        {
            Logger.LogInformation($"OS Build Spec {id} Deleted by {UserSecurity.SamAccountName}");
        };

        protected override Action<OsSpecInformation> LogSave => (info) =>
        {
            Logger.LogInformation($"OS Build Spec {info?.Id} Saved by {UserSecurity.SamAccountName}");
        };      
    }
}