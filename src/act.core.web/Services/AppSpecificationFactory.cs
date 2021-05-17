using System;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Models;
using act.core.web.Models.AppSpecs;
using Microsoft.EntityFrameworkCore;

namespace act.core.web.Services
{
    internal class AppSpecificationFactory : SpecificationFactoryBase<AppSpecInformation, AppSpecSearchResult>,
        ISpecificationFactory<AppSpecInformation, AppSpecSearchResult>
    {
        public AppSpecificationFactory(ActDbContext ctx) : base(ctx, BuildSpecificationTypeConstant.Application)
        {
        }

        public override async Task AddOrUpdate(AppSpecInformation info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            if (!await IsUnique(info))
                throw new ArgumentException(@"The name is not unique", nameof(info));
            BuildSpecification it = null;
            var osSpec = await Ctx.BuildSpecifications.ById(BuildSpecificationTypeConstant.OperatingSystem, info.OsSpecId.GetValueOrDefault());
            if (info.Id.HasValue)
                it = await Ctx.BuildSpecifications.ById(BuildSpecificationTypeConstant.Application,
                    info.Id.Value);


            if (it == null)
            {
                it = Ctx.BuildSpecifications.Add(new BuildSpecification
                {
                    BuildSpecificationType = BuildSpecificationTypeConstant.Application,
                    Name = info.Name,
                    OwnerEmployeeId = info.OwnerId.GetValueOrDefault(),
                    ParentId = info.OsSpecId,
                    WikiLink = info.WikiLink,
                    Overview = info.Overview,
                    EmailAddress = info.Email,
                    RunningCoreOs = osSpec?.Platform == PlatformConstant.WindowsServer  && info.RunningCoreOs.GetValueOrDefault(),
                    IncludeRemedyEmailList = info.IncludeRemedyEmailList
                }).Entity;
            }
            else
            {
                it.Name = info.Name;
                it.OwnerEmployeeId = info.OwnerId.GetValueOrDefault();
                it.RunningCoreOs = osSpec?.Platform== PlatformConstant.WindowsServer && info.RunningCoreOs.GetValueOrDefault();
                it.ParentId = info.OsSpecId;
                it.WikiLink = info.WikiLink;
                it.Overview = info.Overview;
                it.EmailAddress = info.Email;
                it.IncludeRemedyEmailList = info.IncludeRemedyEmailList;
            }

            await Ctx.SaveChangesAsync();
            info.Id = it.Id;
        }

        protected override AppSpecInformation ModelMapper(BuildSpecification it)
        {
            return new AppSpecInformation
            {
                OsSpecId = it.ParentId.GetValueOrDefault(),
                OsSpecName = it.Parent?.Name,
                WikiLink = it.WikiLink,
                Overview = it.Overview,
                OwnerId = it.OwnerEmployeeId,
                OwnerName = it.Owner.OwnerText(),
                Platform = (it.Parent?.Platform).GetValueOrDefault(PlatformConstant.Linux).ToString().ToLower(),
                Name = it.Name,
                Id = it.Id,
                Email = it.EmailAddress,
                RunningCoreOs = it.RunningCoreOs,
                IncludeRemedyEmailList = it.IncludeRemedyEmailList
            };
        }

        protected override async Task<AppSpecSearchResult[]> SearchResultMapper(IQueryable<BuildSpecification> q)
        {
            return (await q
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Owner,
                        OsSpecName = p.Parent.Name,
                        PortCount = p.Ports.Count(),
                        SoftwareCount = p.SoftwareComponents.Count(),
                        JustifiedSoftwareCount = p.SoftwareComponents.Count(s => s.JustificationId != null)
                    })
                    .ToArrayAsync()) //Yank from DB
                .Select(p => new AppSpecSearchResult(p.Id, p.Name, p.Owner.Id, p.Owner.OwnerText(), p.OsSpecName,
                    new Counts(p.PortCount, p.JustifiedSoftwareCount, p.SoftwareCount)))
                .ToArray();
        }
    }
}