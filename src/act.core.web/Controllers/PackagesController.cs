using System;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Extensions;
using act.core.web.Framework;
using act.core.web.Models.Packages;
using act.core.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class PackagesController : PureMvcControllerBase
    {
        private readonly ISoftwareComponentFactory _packageFactory;
        private readonly INodeFactory _nodeFactory;

        public PackagesController(ISoftwareComponentFactory packageFactory, INodeFactory nodeFactory, ILoggerFactory logger) : base(logger)
        {
            _packageFactory = packageFactory;
            _nodeFactory = nodeFactory;
        }

        [HttpPost]
        public async Task<PartialViewResult> ForSpec(JustificationTypeConstant id, BuildSpecificationTypeConstant specType, long specId)
        {
            return PartialView(new PackagesScreen(specType, id, specId, await _packageFactory.GetSoftwareComponents(id, specId)));
        }
        [HttpPost]
        public async Task<PartialViewResult> All(JustificationTypeConstant id, long specId)
        {
            return PartialView(new Packages(await _packageFactory.GetSoftwareComponents(id, specId)));
        }
        [HttpPost]
        public async Task<PartialViewResult> Single(long id)
        {
            return PartialView(await _packageFactory.GetSoftwareComponent(id));
        }

        [HttpPost]
        public async Task<PartialViewResult> Edit(long id)
        {
            var it = await _packageFactory.GetSoftwareComponent(id);
            return PartialView(new EditPackage(it.BuildSpecificationType, it.PackageType, it.Name, it.Description,
                it.NonCore, it.PciScope, it.EnvironmentIds, await _nodeFactory.GetEnvironments()));
        }
        [HttpPost]
        public async Task<PartialViewResult> New(JustificationTypeConstant id, BuildSpecificationTypeConstant specType, long specId)
        {
            return PartialView(new NewPackage(id, specType, specId, await _nodeFactory.GetEnvironments()));
        }

        [HttpPost]
        public PartialViewResult NewBulk(JustificationTypeConstant id, BuildSpecificationTypeConstant specType, long specId)
        {
            return PartialView(new NewBulkPackages(id, specType, specId));
        }
        
        [HttpPost]
        public async Task<JsonResult> Add(JustificationTypeConstant id, long specId, string name, string description, bool? nonCore, PciScopeConstant[] pciScope, int[] environmentIds)
        {
            if (!_packageFactory.IsValidName(name))
                return Json(JsonEnvelope.Error($"The {id} cannot be empty whitespace."));

            var newid = await _packageFactory.AddSoftwareComponent(id, specId, name?.Trim(), description?.Trim(), nonCore.GetValueOrDefault(), pciScope, environmentIds);
            if (newid.HasValue)
            {
                Logger.LogInformation($"{id} {newid} added by {UserSecurity.SamAccountName} for spec {specId} with name {name}");
                return Json(JsonEnvelope.Success(new {url = Url.PartialGetPackage(newid.Value)}));
            }
            return Json(JsonEnvelope.Error($"The {id} named {name} was already found in this spec or its parent."));
        }
        [HttpPost]
        public async Task<JsonResult> Save(long id, string name, string description, bool? nonCore, PciScopeConstant[] pciScope, int[] environmentIds)
        {
            if (!_packageFactory.IsValidName(name))
                return Json(JsonEnvelope.Error("The Name cannot be empty whitespace."));

            await _packageFactory.SaveSoftwareComponent(id, name?.Trim(), description?.Trim(), nonCore.GetValueOrDefault(), pciScope, environmentIds);

            Logger.LogInformation($"Software Component {id} updated by {UserSecurity.SamAccountName} with name {name}");
            return Json(JsonEnvelope.Success(new { url = Url.PartialGetPackage(id) }));
        }
        
        [HttpPost]
        public async Task<JsonResult> BulkAdd(JustificationTypeConstant id, long specId, BulkAddTypeConstant? addType, string[] names, string [] others)
        {
            if (names == null)
                return Json(JsonEnvelope.Error("Nothing was passed in to save."));

            var toAdd = names.Where(p => _packageFactory.IsValidName(p)).Select(p=>p.Trim()).Distinct().ToArray();
            if (toAdd.Length == 0)
                return Json(JsonEnvelope.Error("Nothing was passed in to save."));

            if (addType.GetValueOrDefault(BulkAddTypeConstant.NamesOnly) != BulkAddTypeConstant.NamesOnly &&
                (others == null || others.Length != toAdd.Length || others.All(string.IsNullOrWhiteSpace)))
                return Json(JsonEnvelope.Error("There is a mismatch between distinct names and the second column in excel.  Ensure there are not duplicates and the second column has data for all values in the first column. "));

            string[] skipped;
            switch (addType.GetValueOrDefault(BulkAddTypeConstant.NamesOnly))
            {
                case BulkAddTypeConstant.NamesOnly:
                    skipped = await _packageFactory.BulkAddSoftwareComponents(id, specId, toAdd, null);
                    break;
                case BulkAddTypeConstant.WithDescriptions:
                    skipped = await _packageFactory.BulkAddSoftwareComponents(id, specId, toAdd, others.Select(p=>p?.Trim()).ToArray());
                    break;
                case BulkAddTypeConstant.WithJustifications:
                   
                    skipped = await _packageFactory.BulkAddSoftwareComponentsWithJustifications(id, specId, toAdd, others.Select(p => p?.Trim()).ToArray());
                    break;
                default:
                    throw new ArgumentException($@"Type {addType} not handled.", nameof(addType));
            }

            Logger.LogInformation($"{id} bulk added by {UserSecurity.SamAccountName} for specId {specId} with data: { string.Join(", ", toAdd.Except(skipped))}");
            return Json(JsonEnvelope.Success(new {url = Url.PartialGetPackages(id, specId), skipped, type=id.ToString(), justifications = BulkAddTypeConstant.WithJustifications == addType }));

        }

        [HttpPost]
        public async Task<JsonResult> CleanDuplicates(JustificationTypeConstant id, long specId)
        {
            await _packageFactory.RemoveSoftwareComponentDuplicates(id, specId);
            Logger.LogInformation($"{id} cleaned up by by {UserSecurity.SamAccountName} for specId {specId}");
            return Json(JsonEnvelope.Success(new { url = Url.PartialGetPackages(id, specId)}));        
        }
        [HttpPost]
        public async Task<JsonResult> Assign(long id, long? justificationId)
        {

            Logger.LogInformation($"Package {id} assigned to justification {justificationId} by {UserSecurity.SamAccountName}");
            await _packageFactory.AssignJustification(id, justificationId);
            return Json(JsonEnvelope.Success());
        }

        [HttpPost]
        public async Task<JsonResult> Delete(long id)
        {

            Logger.LogInformation($"Package {id} deleted by {UserSecurity.SamAccountName}");
            await _packageFactory.DeleteSoftwareComponent(id);
            return Json(JsonEnvelope.Success());
        }
    }
}