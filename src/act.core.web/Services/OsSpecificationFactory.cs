using System;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Models;
using act.core.web.Models.OsSpecs;
using Microsoft.EntityFrameworkCore;

namespace act.core.web.Services
{
    internal class OsSpecificationFactory : SpecificationFactoryBase<OsSpecInformation, OsSpecSearchResult>,
        ISpecificationFactory<OsSpecInformation, OsSpecSearchResult>
    {
        public OsSpecificationFactory(ActDbContext ctx) : base(ctx, BuildSpecificationTypeConstant.OperatingSystem)
        {
        }

        public override async Task AddOrUpdate(OsSpecInformation info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            if (!await IsUnique(info))
                throw new ArgumentException(@"The name is not unique", nameof(info));
            BuildSpecification it = null;

            if (info.Id.HasValue)
                it = await Ctx.BuildSpecifications.ById(BuildSpecificationTypeConstant.OperatingSystem,
                    info.Id.Value);

            if (it == null)
            {
                it = Ctx.BuildSpecifications.Add(new BuildSpecification
                {
                    BuildSpecificationType = BuildSpecificationTypeConstant.OperatingSystem,
                    Name = info.Name,
                    OwnerEmployeeId = info.OwnerId.GetValueOrDefault(),
                    OperatingSystemName = info.OsName,
                    OperatingSystemVersion = info.OsVersion,
                    Platform = info.Platform,
                    EmailAddress = info.Email
                }).Entity;
            }
            else
            {
                it.Name = info.Name;
                it.OwnerEmployeeId = info.OwnerId.GetValueOrDefault();
                it.OperatingSystemName = info.OsName;
                it.OperatingSystemVersion = info.OsVersion;
                it.Platform = info.Platform;
                it.EmailAddress = info.Email;
            }

            await Ctx.SaveChangesAsync();
            info.Id = it.Id;
        }

        protected override OsSpecInformation ModelMapper(BuildSpecification it)
        {
            return new OsSpecInformation
            {
                OsName = it.OperatingSystemName,
                OsVersion = it.OperatingSystemVersion,
                OwnerId = it.OwnerEmployeeId,
                OwnerName = it.Owner.OwnerText(),
                Platform = it.Platform.GetValueOrDefault(),
                Name = it.Name,
                Email = it.EmailAddress,
                Id = it.Id
            };
        }


        protected override async Task<OsSpecSearchResult[]> SearchResultMapper(IQueryable<BuildSpecification> q)
        {
            return (await q
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Owner,
                        p.Platform,
                        PortCount = p.Ports.Count(),
                        SoftwareCount = p.SoftwareComponents.Count(),
                        JustifiedSoftwareCount = p.SoftwareComponents.Count(s => s.JustificationId != null)
                    })
                    .ToArrayAsync()) //Yank from DB
                .Select(p => new OsSpecSearchResult(p.Id, p.Name, p.Owner.Id, p.Owner.OwnerText(),
                    p.Platform.GetValueOrDefault(),
                    new Counts(p.PortCount, p.JustifiedSoftwareCount, p.SoftwareCount)))
                .ToArray();
        }
    }
}