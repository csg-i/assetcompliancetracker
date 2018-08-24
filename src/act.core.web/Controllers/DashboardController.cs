using System;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Extensions;
using act.core.web.Framework;
using act.core.web.Models.Dashboard;
using act.core.web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace act.core.web.Controllers
{
    
    public class DashboardController : PureMvcControllerBase
    {
        private readonly IDashboardFactory _dashboardFactory;

        public DashboardController(IDashboardFactory dashboardFactory, ILoggerFactory logger) : base(logger)
        {
            _dashboardFactory = dashboardFactory;
        }
  
        public async Task<JsonResult> ComplianceOverTime(int? daysBack, bool? pciOnly, long? employeeId, ComplianceOverTime.EmployeeFilterConstant? filterType)
        {
            var data = await _dashboardFactory.ComplianceOverTime(daysBack.GetValueOrDefault(14),
                pciOnly.GetValueOrDefault(), employeeId, filterType.GetValueOrDefault());
            return Json(JsonEnvelope.Success(new
            {
                categories = data.Categories,
                weekends = data.FindWeekendGroups().ToArray(),
                series = new[]
                {
                    new { name = "Passing", data = data.GetByStatus(ComplianceStatusConstant.Succeeded).ToArray() },
                    new { name = "Failing", data = data.GetByStatus(ComplianceStatusConstant.Failed).ToArray() }
                }
            }));
        }
        public async Task<JsonResult> Spread()
        {
            var spread = await _dashboardFactory.Spread();
            return Json(JsonEnvelope.Success(new
            {
                product= new
                {
                    categories = spread.ProductSpecs.Select(p=>p.Name).ToArray(),
                    series = new[]
                    {
                        new { name="# of Specs", data=spread.ProductSpecs.Select(p=>p.SpecCount).ToArray() },
                        new { name="# of Nodes", data=spread.ProductSpecs.Select(p=>p.NodeCount).ToArray() },
                    }
                },
                os = new
                {
                    categories = spread.OsSpecs.Select(p => p.Name).ToArray(),
                    series = new[]
                    {
                        new { name="# of Specs", data=spread.OsSpecs.Select(p=>p.SpecCount).ToArray() },
                        new { name="# of Nodes", data=spread.OsSpecs.Select(p=>p.NodeCount).ToArray() },
                    }
                }
            }));
        }
        public async Task<JsonResult> TopOffenders()
        {
            var offenders = await _dashboardFactory.TopOffenders();
            var names = offenders.Select(p => p.Description).ToList();
            names.Add("OS Name");
            var values = offenders.Select(p => p.Count).ToList();
            values.Add(names.Count);
            return Json(JsonEnvelope.Success(new {
                categories = names.ToArray(),
                series = new[]
                {
                    new {name="Top Offenders", data=values.ToArray()},
                }
            }));
        }

        public async Task<JsonResult> DirectorChart()
        {
            var data = await _dashboardFactory.GetDirectorPciScoreChart();
            return Json(JsonEnvelope.Success(new
            {
                categories = data.Names,
                series = new[]
                {
                    new {name="Not Reporting", data=data.NotReporting},
                    new {name="Failing", data=data.Failing},
                    new {name="Passing", data=data.Passing},
                }
            }));
        }
        public async Task<JsonResult> Status()
        {
            var status = await _dashboardFactory.Status();
            return Json(JsonEnvelope.Success(new
            {
                outOfScope = new[]
                {
                    new{name="Appliance", data= new []{ status.Applicance.PciCount, status.Applicance.TotalCount}, lockrType=PlatformConstant.Appliance.ToString(), url=Url.NodeSearch()},
                    new{name="UNIX", data= new []{ status.Unix.PciCount, status.Unix.TotalCount}, lockrType=PlatformConstant.Unix.ToString(), url=Url.NodeSearch()},
                    new{name="Product", data= new []{ status.ProductExcluded.PciCount, status.ProductExcluded.TotalCount}, lockrType="", url=Url.NodesExcludedByProductReport()},
                    new{name="Other OS", data= new []{ status.OsOther.PciCount, status.OsOther.TotalCount}, lockrType=PlatformConstant.Other.ToString(), url=Url.NodeSearch()},
                },
                failing = new[]
                {
                    new{name="Compliance Failure", data= new []{ status.FailingCompliance.PciCount, status.FailingCompliance.TotalCount}, lockrType=Models.Nodes.NodeComplianceSearchTypeConstant.Failing.ToString(), url=Url.NodeSearch()},
                    new{name="Not Reporting", data= new []{ status.NotReporting.PciCount, status.NotReporting.TotalCount}, lockrType=Models.Nodes.NodeComplianceSearchTypeConstant.NotReporting.ToString(), url=Url.NodeSearch()},
                    new{name="Not Assigned", data= new []{ status.Unassigned.PciCount, status.Unassigned.TotalCount}, lockrType=Models.Nodes.NodeComplianceSearchTypeConstant.Unassigned.ToString(), url=Url.NodeSearch()},
                },
                inScope = new[]
                {
                    new{ type="pie", name="PCI", url=Url.NodeSearch(), data= new []{ new {name="Passing", y= status.Passing.PciCount }, new { name="Failing", y=status.TotalFailing.PciCount} }, center= new[]{"25%","50%"},size="50%"},
                    new{ type="pie", name="All", url=Url.NodeSearch(), data= new []{ new {name= "Passing", y= status.Passing.TotalCount }, new { name="Failing", y=status.TotalFailing.TotalCount } }, center= new[]{"75%","50%"},size="50%"}
                }
            }));
        }
    }
}