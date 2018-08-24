using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.web.Extensions;
using act.core.web.Framework;
using act.core.web.Models.Home;
using act.core.web.Models.ScoreCard;
using act.core.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class ScoreCardController : PureMvcControllerBase
    {
        private readonly IScoreCardFactory _scoreCardFactory;
        private readonly IExcelExporter _exporter;

        public ScoreCardController(IScoreCardFactory scoreCardFactory, IExcelExporter exporter, ILoggerFactory logger) : base(logger)
        {
            _scoreCardFactory = scoreCardFactory;
            _exporter = exporter;
        }

        public ViewResult Home()
        {
            return View(new Home());
        }

        public ViewResult Owner()
        {
            return View(new OwnerScoreCardScreen(UserSecurity.EmployeeId, UserSecurity.OwnerText()));
        }
        public ViewResult Executive()
        {
            return View(new ExecutiveScoreCardScreen(UserSecurity.EmployeeId, UserSecurity.OwnerText()));
        }
        public ViewResult Platform()
        {
            return View(new PlatformScoreCardScreen());
        }

        public ViewResult Product()
        {
            return View(new ProductScoreCardScreen());
        }
        public ViewResult Director()
        {
            return View(new DirectorScoreCardScreen());
        }
        [HttpPost]
        public async Task<PartialViewResult> OwnerData(long id)
        {
            return PartialView(await _scoreCardFactory.GetOwnerScoreCard(id));
        }
        [HttpGet]
        public async Task<FileContentResult> OwnerExport(long id)
        {
            SetNoCacheHeader();

            var scoreCard = await _scoreCardFactory.GetOwnerScoreCard(id);
            var file = _exporter.Export(scoreCard);
            return File(file, "text/csv", $"owner_sc_{id}.csv");
        }
        [HttpPost]
        public async Task<PartialViewResult> ExecutiveData(long id)
        {
            return PartialView(await _scoreCardFactory.GetExecutiveScoreCard(id));
        }
        [HttpGet]
        public async Task<FileContentResult> ExecutiveExport(long id)
        {
            SetNoCacheHeader();
            var scoreCard = await _scoreCardFactory.GetExecutiveScoreCard(id);
            return File(_exporter.Export(scoreCard), "text/csv", $"executive_sc_{id}.csv");
        }

        [HttpPost]
        public async Task<PartialViewResult> PlatformData()
        {
            return PartialView(await _scoreCardFactory.GetPlatformScoreCard());
        }

        [HttpGet]
        public async Task<FileContentResult> PlatformExport()
        {
            SetNoCacheHeader();

            var scoreCard = await _scoreCardFactory.GetPlatformScoreCard();
            var file = _exporter.Export(scoreCard);
            return File(file, "text/csv", $"platform_sc_.csv");
        }
        [HttpPost]
        public async Task<JsonResult> Products()
        {
            var data = await _scoreCardFactory.GetProductCodes();
            var urls = data.Select(p => Url.PartialProductScoreCard(p));
            return Json(JsonEnvelope.Success( new {urls} ) );
        }

        [HttpPost]
        public async Task<PartialViewResult> ProductData(string id)
        {
            return PartialView(await _scoreCardFactory.GetProductScoreCard(id));
        }
        [HttpGet]
        public async Task<FileContentResult> ProductExport()
        {
            SetNoCacheHeader();
            var data = await _scoreCardFactory.GetProductCodes();
            var list = new List<IProductScoreCard>();
            foreach (var d in data)
            {
                var scoreCard = await _scoreCardFactory.GetProductScoreCard(d);
                scoreCard.ForEach(p=>p.SetCode(scoreCard));
                list.Add(scoreCard);
                list.AddRange(scoreCard);

            }
            var file = _exporter.Export(list);
            return File(file, "text/csv", $"product_sc_.csv");
        }
        [HttpPost]
        public async Task<PartialViewResult> DirectorData()
        {
            return PartialView(await _scoreCardFactory.GetDirectorScoreCard());
        }
        [HttpGet]
        public async Task<FileContentResult> DirectorExport()
        {
            SetNoCacheHeader();
            var list = new List<IDirectorScoreCard>();
            var scoreCard = await _scoreCardFactory.GetDirectorScoreCard();
            list.Add(scoreCard);
            list.AddRange(scoreCard);
            var file = _exporter.Export(list);
            return File(file, "text/csv", $"director_sc_.csv");
        }
    }
}