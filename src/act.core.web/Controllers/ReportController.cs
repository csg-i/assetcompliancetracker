using System.Threading.Tasks;
using act.core.web.Framework;
using act.core.web.Models.Home;
using act.core.web.Models.Report;
using act.core.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class ReportController:PureMvcControllerBase
    {
        private readonly INodeFactory _nodeFactory;

        public ReportController(ILoggerFactory logger, INodeFactory nodeFactory) : base(logger)
        {
            _nodeFactory = nodeFactory;
        }
        
        public ViewResult Home()
        {
            return View(new Home());
        }

        public ViewResult Ports(long id)
        {
            return View(new PortReportScreen(id));
        }

        public ViewResult AssignedNodes()
        {
            return View(new AssignedNodesReportScreen());
        }

        public ViewResult AllPorts()
        {
            return View(new PortsReportScreen());
        }
        public ViewResult Owners()
        {
            return View(new OwnerReportScreen());
        }
        public async Task<ViewResult> Review(long id, int? environmentId)
        {
            return View(new Review(id, environmentId, await _nodeFactory.GetEnvironments()));
        }
        
        public ViewResult NotReporting()
        {
            return View(new NotReporting());
        }
        
        public ViewResult ExcludedProduct()
        {
            return View(new ExcludedProduct());
        }

    }
}