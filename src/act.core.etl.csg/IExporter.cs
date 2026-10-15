using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using Microsoft.EntityFrameworkCore;

namespace act.core.etl.csg
{
    public interface IExporter
    {
        Task<NodeData[]> Nodes();
        Task<Dictionary<string, SpecId[]>> Software();
    }

    class Exporter : IExporter
    {
        private readonly ActDbContext _ctx;

        public Exporter(ActDbContext ctx)
        {
            _ctx = ctx;
        }
        
        public async Task<NodeData[]> Nodes()
        {
            return await _ctx.ExecuteReaderAnync(Sql.NodeDump, reader => new NodeData()
            {
                GamInventoryId = reader.GetInt64(0),
                AppSpecId = reader.GetNullableFieldValue<long>(1),
                AppSpecName = reader.GetNullableString(2),
                AppSpecOwnerLanId = reader.GetNullableString(3),
                AppSpecOwnerName = reader.GetNullableString(4),
                OsSpecId = reader.GetNullableFieldValue<long>(5),
                OsSpecName = reader.GetNullableString(6),
                Os = reader.GetNullableString(7),
                OsVersion = reader.GetNullableString(8),
                OsSpecOwnerLanId = reader.GetNullableString(9),
                OsSpecOwnerName = reader.GetNullableString(10),
                NodeEnvironmentName = reader.GetNullableString(11),
                ChefNodeId = reader.GetNullableFieldValue<Guid>(12),
                ComplianceStatus = reader.GetNullableString(13),
                LastComplianceResultDate = reader.GetNullableFieldValue<DateTime>(14),
                Fqdn = reader.GetString(15),
                IsAppliance = reader.GetInt32(16) > 0,
                IsUnixPhysical = reader.GetInt32(17) > 0,
                PciScope = reader.GetString(18),
                GamTechContactLanId = reader.GetString(19).TrimEnd(),
                GamTechContactName = reader.GetString(20),
                RemedyGroupName = reader.GetString(21),
                RemedyGroupEmailList = reader.GetString(22)
            });

        }

        public async Task<Dictionary<string, SpecId[]>> Software()
        {
            var software = await _ctx.SoftwareComponents
                .AsNoTracking()
                .Select(s => new SpecId
                {
                    Name = s.Name.ToLower(),
                    VersionDescription = s.Description,
                    Id = s.BuildSpecificationId,
                    SpecTypeId = s.BuildSpecification.BuildSpecificationType
                }).ToArrayAsync();

            var unique = await _ctx.SoftwareComponents.AsNoTracking().Select(p => p.Name).Distinct().ToArrayAsync();

            return unique.ToDictionary(p => p, p => software.Where(s => s.Name == p.ToLower()).ToArray());
        }
    }
}