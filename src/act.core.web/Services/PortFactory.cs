using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Extensions;
using act.core.web.Models.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace act.core.web.Services
{
    public interface IPortFactory
    {
        Task<SimplePort[]> GetPorts(long specId);

        Task AddOrUpdatePorts(long specId, long justificationId, bool external, bool outgoing,
            PortTypeConstant portType,
            string portString);

        DataPortValidation Validate(string portString);

        Task<SimplePort> GetPort(long justificationId);

        Task DeletePort(long justificationId);
    }

    internal class PortFactory : IPortFactory
    {
        private readonly ActDbContext _ctx;
        private readonly ILogger _logger;

        public PortFactory(ActDbContext ctx, ILoggerFactory loggerFactory)
        {
            _ctx = ctx;
            _logger = loggerFactory.CreateLogger<PortFactory>();
        }

        public DataPortValidation Validate(string portString)
        {
            return portString.ValidateDataPorts();
        }


        public async Task<SimplePort> GetPort(long justificationId)
        {
            {
                var justification = await _ctx.Justifications.AsNoTracking().ById(justificationId);
                if (justification == null)
                    return null;

                return await SimplePortInternal(justification);
            }
        }

        public async Task<SimplePort[]> GetPorts(long specId)
        {
            var list = new List<SimplePort>();
            var justifications = await _ctx.Ports.AsNoTracking()
                .ForBuildSpec(specId)
                .Justified()
                .Select(p => p.Justification)
                .Distinct()
                .ToArrayAsync();
            foreach (var j in justifications)
            {
                var it = await SimplePortInternal(j);
                if (it != null)
                    list.Add(it);
            }

            return list.ToArray();
        }

        public async Task AddOrUpdatePorts(long specId, long justificationId, bool external, bool outgoing,
            PortTypeConstant portType, string portString)
        {
            var valid = portString.ValidateDataPorts().Ports;
            var inDb = _ctx.Ports.ForJustification(justificationId);
            _ctx.Ports.RemoveRange(inDb);

            foreach (var p in valid)
            {
                p.JustificationId = justificationId;
                p.PortType = portType;
                p.BuildSpecificationId = specId;
                p.IsExternal = external || outgoing;
                p.IsOutgoing = outgoing;
                _ctx.Ports.Add(p);
            }

            await _ctx.SaveChangesAsync();
        }

        public async Task DeletePort(long justificationId)
        {
            var inDb = _ctx.Ports.ForJustification(justificationId);
            var count = await inDb.CountAsync();
            _ctx.Ports.RemoveRange(inDb);
            await _ctx.SaveChangesAsync();
            _logger.LogInformation($"Successfully deleted {count} port for justification {justificationId}");
        }

        private async Task<SimplePort> SimplePortInternal(Justification j)
        {
            var ports = await _ctx
                .Ports.AsNoTracking()
                .ForJustification(j.Id)
                .OrderBy(p => p.From)
                .ThenBy(p => p.To)
                .ToArrayAsync();

            var first = ports.FirstOrDefault();

            if (first == null)
                return null;

            return new SimplePort(ports.ToPortString(), first.PortType, first.IsExternal, first.IsOutgoing,
                j.JustificationText, j.Id);
        }
    }
}