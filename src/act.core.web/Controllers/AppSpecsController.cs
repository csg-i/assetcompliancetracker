using System;
using act.core.data;
using act.core.web.Models.AppSpecs;
using act.core.web.Services;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class AppSpecsController : SpecsControllerBase<AppSpecInformation,AppSpecSearchResult, AppSpecSearchResults>
    {
        
        protected override BuildSpecificationTypeConstant BuildSpecificationType => BuildSpecificationTypeConstant.Application;

        protected override Action<long> LogDelete => (id) =>
        {
            Logger.LogInformation($"Application Build Spec Deleted by {UserSecurity.SamAccountName} with id of {id}");
        };

        protected override Action<AppSpecInformation> LogSave => (info) =>
        {
            Logger.LogInformation($"Application Build Spec Saved by {UserSecurity.SamAccountName} with id of {info?.Id}");
        };

        public AppSpecsController(ISpecificationFactory<AppSpecInformation, AppSpecSearchResult> specificationFactory, ILoggerFactory logger) : base(specificationFactory, logger)
        {
        }     
    }
}