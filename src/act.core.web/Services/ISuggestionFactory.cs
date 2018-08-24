using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Models.BuildSpec;
using Microsoft.EntityFrameworkCore;

namespace act.core.web.Services
{
    public interface ISuggestionFactory
    {
        Task<Suggestions> ComplianceDataSuggestions(long specId, JustificationTypeConstant resultType, bool shouldExist,
            PortTypeConstant? portType, string value);
    }

    public class SuggestionFactory : ISuggestionFactory
    {
        private readonly ActDbContext _ctx;

        public SuggestionFactory(ActDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Suggestions> ComplianceDataSuggestions(long specId, JustificationTypeConstant resultType,
            bool shouldExist, PortTypeConstant? portType, string value)
        {
            var spec = await _ctx.BuildSpecifications.AsNoTracking()
                .Include(p => p.Parent)
                .Include(p => p.Parent.Owner)
                .Include(p => p.Owner)
                .ById(specId);
            if (spec == null)
                return Suggestions.Empty;

            if (InvalidPlatform(spec.Platform ?? spec.Parent?.Platform, resultType, portType))
                return Suggestions.NodeMismatch;

            var list = new List<Suggestion>();
            if (!shouldExist)
            {
                //2 - 4 ways to add it
                list.AddRange(CreateAdd(spec, spec.Owner, resultType, value, portType));
                if (spec.BuildSpecificationType == BuildSpecificationTypeConstant.Application &&
                    spec.ParentId.HasValue)
                    list.AddRange(CreateAdd(spec.Parent, spec.Parent?.Owner, resultType, value, portType));
            }
            else
            {
                //can only remove it if found.
                if (resultType == JustificationTypeConstant.Port)
                {
                    list.AddRange(CreateRemove(spec, spec.Owner, resultType, value,
                        await FindPort(spec.Id, portType, value), portType));
                    if (spec.BuildSpecificationType == BuildSpecificationTypeConstant.Application &&
                        spec.ParentId.HasValue)
                    {
                        var parent = spec.Parent;
                        if (parent != null)
                            list.AddRange(CreateRemove(parent, parent.Owner, resultType, value,
                                await FindPort(parent.Id, portType, value), portType));
                    }
                }
                else
                {
                    list.AddRange(CreateRemove(spec, spec.Owner, resultType, value,
                        await FindSoftware(spec.Id, resultType, value)));
                    if (spec.BuildSpecificationType == BuildSpecificationTypeConstant.Application &&
                        spec.ParentId.HasValue)
                    {
                        var parent = spec.Parent;
                        if (parent != null)
                            list.AddRange(CreateRemove(parent, parent.Owner, resultType, value,
                                await FindSoftware(parent.Id, resultType, value)));
                    }
                }
            }

            if (!list.Any())
                return Suggestions.Empty;

            return new Suggestions(spec.BuildSpecificationType, spec.Id, spec.Name, list);
        }

        private bool InvalidPlatform(PlatformConstant? platform, JustificationTypeConstant type,
            PortTypeConstant? port)
        {
            bool valid;
            var p = platform.GetValueOrDefault();
            switch (p)
            {
                case PlatformConstant.Linux:
                case PlatformConstant.Unix:
                    valid = type == JustificationTypeConstant.Port || type == JustificationTypeConstant.Package;
                    break;
                case PlatformConstant.WindowsServer:
                    valid = type != JustificationTypeConstant.Package;
                    break;
                case PlatformConstant.WindowsClient:
                    valid = type != JustificationTypeConstant.Port || type == JustificationTypeConstant.Application;
                    break;
                default:
                    valid = type == JustificationTypeConstant.Port;
                    break;
            }

            if (!valid)
                return true;

            if (type == JustificationTypeConstant.Port)
            {
                if (port == null)
                    return true;

                if (p == PlatformConstant.WindowsClient || p == PlatformConstant.WindowsServer || p == PlatformConstant.Other)
                    if (port.Value == PortTypeConstant.Tcp6 || port.Value == PortTypeConstant.Udp6)
                        return true;
            }

            return false;
        }

        private IEnumerable<Suggestion> CreateAdd(BuildSpecification spec, Employee owner,
            JustificationTypeConstant resultType, string value,
            PortTypeConstant? portType = null)
        {
            yield return new Suggestion(spec.BuildSpecificationType, spec.Id, spec.Name,
                owner.OwnerText(false), owner.SamAccountName, resultType, portType, value, false);

            yield return new Suggestion(spec.BuildSpecificationType, spec.Id, spec.Name,
                owner.OwnerText(false), owner.SamAccountName, resultType, portType, value, true);
        }

        private IEnumerable<Suggestion> CreateRemove(BuildSpecification spec, Employee owner,
            JustificationTypeConstant resultType, string value, BuildSpecReference found,
            PortTypeConstant? portType = null)
        {
            if (found != null)
            {
                yield return new Suggestion(spec.BuildSpecificationType, spec.Id, spec.Name,
                    owner.OwnerText(false), owner.SamAccountName, resultType, portType, value, false, found);

                yield return new Suggestion(spec.BuildSpecificationType, spec.Id, spec.Name,
                    owner.OwnerText(false), owner.SamAccountName, resultType, portType, value, true, found);
            }
        }

        private async Task<BuildSpecReference> FindPort(long specId, PortTypeConstant? portType, string value)
        {
            int port;
            if (int.TryParse(value, out port))
                return await _ctx.Ports.AsNoTracking().ForBuildSpec(specId)
                    .Where(p => p.PortType == portType)
                    .FirstOrDefaultAsync(p => p.From == port || p.To == port || p.From < port && p.To > port);

            return null;
        }

        private async Task<BuildSpecReference> FindSoftware(long specId, JustificationTypeConstant resultType,
            string value)
        {
            return await _ctx.SoftwareComponents.AsNoTracking().ForBuildSpec(specId)
                .Where(p => p.JustificationType == resultType)
                .FirstOrDefaultAsync(p => p.Name == value);
        }
    }
}