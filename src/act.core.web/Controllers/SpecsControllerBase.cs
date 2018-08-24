using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Extensions;
using act.core.web.Framework;
using act.core.web.Models;
using act.core.web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    public abstract class SpecsControllerBase<TInfo,TSearch, TCollection> : PureMvcControllerBase 
        where TInfo : class, ISpecInformation , new()
        where TSearch : class, ISpecSearchResult
        where TCollection: List<TSearch>, new()
    {
        protected readonly ISpecificationFactory<TInfo, TSearch> SpecFactory;
        protected abstract BuildSpecificationTypeConstant BuildSpecificationType{ get; }

        protected abstract Action<long> LogDelete { get; }
        
        protected abstract Action<TInfo> LogSave { get; }


        protected SpecsControllerBase(ISpecificationFactory<TInfo,TSearch> specFactory, ILoggerFactory logger) : base(logger)
        {
            SpecFactory = specFactory;
        }

        [HttpGet]
        public ViewResult Index()
        {
            return View(new SpecHome());
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<PartialViewResult> Search(SpecSearchTypeConstant t, string q)
        {
            var results = await SpecFactory.GetSearchResults(t, q, UserSecurity);
            var col = new TCollection();
            col.AddRange(results);
            if (IsLoggedIn)
            {
                col.ForEach(p=>p.LoggedInEmployeeId = UserSecurity.EmployeeId);
            }
            return PartialView(col);
        }

        [HttpPost]
        public async Task<JsonResult> TypeAheadSearch(string q)
        {
            if (q?.Length < 2)
                return Json(JsonEnvelope.Error("Waiting for input..."));

            var data = await SpecFactory.TypeAheadSearch(q);
            if (data.Length > 0)
                return Json(JsonEnvelope.Success(data));

            return Json(JsonEnvelope.Error("No results found"));
        }

        [HttpGet]
        public ViewResult Wizard(long? id, bool? fromClone)
        {
            return View(new SpecWizard(BuildSpecificationType, id, fromClone.GetValueOrDefault()));
        }

        [HttpPost]
        public async Task<PartialViewResult> Information(long? specId)
        {
            if (specId != null)
            {
                var it = await SpecFactory.GetOne(specId.Value);
                if (it != null)
                   return PartialView(it);
            }
            
            return PartialView(new TInfo
            {
                OwnerName = UserSecurity.OwnerText(),
                OwnerId = UserSecurity.EmployeeId
            });
        }

        [HttpPost]
        public PartialViewResult Done(long id)
        {
            return PartialView(new SpecDone(id, BuildSpecificationType));
        }
        
        [HttpPost]
        public async Task<JsonResult> Save(TInfo info)
        {
            if (info == null)
                return Json(JsonEnvelope.Error("No data passed in to save."));

            var errors = info.Validate();
            if (errors.Count > 0)
                return Json(JsonEnvelope.Error(errors));

            if (!await SpecFactory.IsUnique(info))
            {
                errors.Add("name", "The Specification Name has already been used.  Please change the name and try again.");
                return Json(JsonEnvelope.Error(errors));
            }

            await SpecFactory.AddOrUpdate(info);
            LogSave(info);
            return Json(JsonEnvelope.Success(new { id = info.Id }));

        }

        [HttpPost]
        public async Task<JsonResult> Delete(long id)
        {
            string reason = await SpecFactory.Delete(id, UserSecurity);
            if (reason == null)
            {
                LogDelete(id);
                return Json(JsonEnvelope.Success());
            }
            return Json(JsonEnvelope.Error(reason));      
        }

        [HttpPost]
        public async Task<JsonResult> Clone(long id)
        {
            var newId = await SpecFactory.Clone(id, UserSecurity);

            return Json(JsonEnvelope.Success(new
            {
                url = BuildSpecificationType == BuildSpecificationTypeConstant.Application
                    ? Url.AppSpecsWizardFromClone(newId)
                    : Url.OsSpecsWizardFromClone(newId)
            }));
        }
    }
}