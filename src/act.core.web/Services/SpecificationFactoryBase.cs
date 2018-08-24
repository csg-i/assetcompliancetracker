using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Framework;
using act.core.web.Models;
using Microsoft.EntityFrameworkCore;

namespace act.core.web.Services
{
    internal abstract class SpecificationFactoryBase<TModel, TSearchResult>
        where TModel : class, ISpecInformation
        where TSearchResult : class, ISpecSearchResult
    {
        private readonly BuildSpecificationTypeConstant _buildSpecificationType;
        protected readonly ActDbContext Ctx;

        protected SpecificationFactoryBase(ActDbContext ctx, BuildSpecificationTypeConstant buildSpecificationType)
        {
            Ctx = ctx;
            _buildSpecificationType = buildSpecificationType;
        }

        public async Task<bool> IsUnique(TModel info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            return await Ctx.BuildSpecifications.IsUnique(info.Id, info.Name);
        }


        public abstract Task AddOrUpdate(TModel info);

        protected abstract TModel ModelMapper(BuildSpecification spec);

        public async Task<TModel> GetOne(long id)
        {
            var it = await Ctx.BuildSpecifications.Include(p => p.Parent).Include(p => p.Owner)
                .ById(_buildSpecificationType, id);

            if (it == null)
                return null;

            return ModelMapper(it);
        }

        protected abstract Task<TSearchResult[]> SearchResultMapper(IQueryable<BuildSpecification> specs);

        public async Task<TSearchResult[]> GetSearchResults(SpecSearchTypeConstant type, string query,
            IUserSecurity employeeSecurity)
        {
            var q = Ctx.BuildSpecifications.AsNoTracking().OfBuildSpecType(_buildSpecificationType);
            switch (type)
            {
                case SpecSearchTypeConstant.Mine:
                    if (employeeSecurity == null)
                    {
                        q = q.Where(p => 1 == 2);
                    }
                    else
                    {
                        var contains = $"%{query}%";
                        q = q.Where(p => p.OwnerEmployeeId == employeeSecurity.EmployeeId);
                        if (!string.IsNullOrWhiteSpace(query))
                            q = q.Where(p => EF.Functions.Like(p.Name, contains)).OrderBy(p => p.Name);
                    }

                    break;
                case SpecSearchTypeConstant.Name:
                    if (employeeSecurity == null)
                    {
                        q = q.Where(p => 1 == 2);
                    }
                    else
                    {
                        var contains = $"%{query}%";
                        q = q.Where(p => EF.Functions.Like(p.Name, contains)).OrderBy(p => p.Name);
                    }
                    
                    break;
                case SpecSearchTypeConstant.Owner:
                    if (string.IsNullOrWhiteSpace(query))
                    {
                        q = q.Where(p => 1 == 2);
                    }
                    else
                    {
                        var empIds = Ctx.Employees.Search(query).Select(p => p.Id).ToArray();
                        q = q.Where(p => empIds.Contains(p.OwnerEmployeeId)).OrderBy(p => p.Owner.FirstName)
                            .ThenBy(p => p.Owner.LastName).ThenBy(p => p.Name);
                    }

                    break;
                case SpecSearchTypeConstant.OsSpec:
                    if (string.IsNullOrWhiteSpace(query) || _buildSpecificationType == BuildSpecificationTypeConstant.OperatingSystem)
                    {
                        q = q.Where(p => 1 == 2);                        
                    }
                    else
                    {
                        var sw = $"{query}%";
                        q = q.Where(p => EF.Functions.Like(p.Parent.Name, sw));
                    }

                    break;
                default:
                    throw new ArgumentException(@"SpecSearchTypeConstant not supported.", nameof(type));
            }

            return await SearchResultMapper(q);
        }

        public async Task<JsonSpecSearchResult[]> TypeAheadSearch(string query)
        {
            if(string.IsNullOrWhiteSpace(query))
                return new JsonSpecSearchResult[0];

            var contains = $"%{query}%";
            return (await Ctx.BuildSpecifications.AsNoTracking()
                    .OfBuildSpecType(_buildSpecificationType)
                    .Where(p => EF.Functions.Like(p.Name, contains))
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        Platform = p.ParentId == null ? p.Platform : p.Parent.Platform
                    })
                    .OrderBy(p => p.Name)
                    .Take(10)
                    .ToArrayAsync())
                .Select(p => new JsonSpecSearchResult(p.Id, p.Name,
                    p.Platform.GetValueOrDefault(PlatformConstant.Unix).ToString().ToLower()))
                .ToArray();
        }

        public async Task<string> Delete(long id, IUserSecurity user)
        {
            var type = _buildSpecificationType == BuildSpecificationTypeConstant.OperatingSystem ? "OS" : "application";
            var toDelete = await Ctx.BuildSpecifications
                .Include(p => p.Nodes)
                .Include(p => p.Children)
                .ById(_buildSpecificationType, id);
            
            if (toDelete == null) 
                return $"The {type} spec not found.";

            if (toDelete.OwnerEmployeeId != user.EmployeeId)
                return $"You are not authorized to delete this {type} spec.";

            if (_buildSpecificationType == BuildSpecificationTypeConstant.Application && toDelete.Nodes.Any())
                return
                    "Some nodes have been assigned to this application spec and must be removed before allowing delete to occur.";

            if (_buildSpecificationType == BuildSpecificationTypeConstant.OperatingSystem && toDelete.Children.Any())
                return
                    "Some application specs use this OS spec and must be changed before allowing the delete to occur.";

            Ctx.SoftwareComponentEnvironments.RemoveRange(Ctx.SoftwareComponentEnvironments.Where(p => p.SoftwareComponent.BuildSpecificationId == toDelete.Id));
            Ctx.SoftwareComponents.RemoveRange(Ctx.SoftwareComponents.ForBuildSpec(toDelete.Id));
            Ctx.Justifications.RemoveRange(Ctx.Justifications.ForBuildSpec(toDelete.Id));
            Ctx.Ports.RemoveRange(Ctx.Ports.ForBuildSpec(toDelete.Id));
            Ctx.BuildSpecifications.Remove(toDelete);
            await Ctx.SaveChangesAsync();

            return null;
        }

        public async Task<long> Clone(long cloneId, IUserSecurity employeeSecurity)
        {
            var toClone = await Ctx.BuildSpecifications
                .Include(p => p.Justifications)
                .Include(p => p.Ports)
                .Include(p => p.SoftwareComponents)
                .ById(_buildSpecificationType, cloneId);
            if (toClone == null)
                throw new ArgumentException(
                    $@"The id passed in ({
                            cloneId
                        }) is not a valid id for a BuildSpec of Type {_buildSpecificationType}",
                    nameof(cloneId));

            var newSpec = new BuildSpecification
            {
                BuildSpecificationType = _buildSpecificationType,
                Name = $"Clone {Guid.NewGuid()}",
                WikiLink = toClone.WikiLink,
                Overview = toClone.Overview,
                OperatingSystemVersion = toClone.OperatingSystemVersion,
                OperatingSystemName = toClone.OperatingSystemName,
                OwnerEmployeeId = employeeSecurity.EmployeeId,
                ParentId = toClone.ParentId,
                Platform = toClone.Platform,
                RunningCoreOs = toClone.RunningCoreOs
            };

            Ctx.BuildSpecifications.Add(newSpec);
            await Ctx.SaveChangesAsync();
            var id = newSpec.Id;
            var jidDict = new Dictionary<long, Justification>();
            foreach (var justification in toClone.Justifications)
            {
                var newJust = Ctx.Justifications.Add(new Justification
                {
                    BuildSpecificationId = id,
                    Color = justification.Color,
                    JustificationText = justification.JustificationText,
                    JustificationType = justification.JustificationType
                }).Entity;
                jidDict.Add(justification.Id, newJust);
            }

            await Ctx.SaveChangesAsync();

            foreach (var port in toClone.Ports)
            {
                var newPort = new Port
                {
                    BuildSpecificationId = id,
                    From = port.From,
                    To = port.To,
                    IsExternal = port.IsExternal,
                    PortType = port.PortType
                };
                if (port.Justification != null) newPort.JustificationId = jidDict[port.Justification.Id].Id;

                Ctx.Ports.Add(newPort);
            }
            
            var sidDict = new Dictionary<long, SoftwareComponent>();
            foreach (var software in toClone.SoftwareComponents)
            {
                var newSoftware = new SoftwareComponent
                {
                    BuildSpecificationId = id,
                    JustificationType = software.JustificationType,
                    Name = software.Name,
                    Description = software.Description,
                    NonCore = software.NonCore,
                    PciScope = software.PciScope
                };
                
                if (software.Justification != null) 
                    newSoftware.JustificationId = jidDict[software.Justification.Id].Id;

                Ctx.SoftwareComponents.Add(newSoftware);
                sidDict.Add(software.Id, newSoftware);
            }

            await Ctx.SaveChangesAsync();

            foreach (var key in sidDict.Keys)
            {
                var environments = Ctx.SoftwareComponentEnvironments.Where(p => p.SoftwareComponentId == key)
                    .Select(p => p.EnvironmentId).ToArray();
                foreach (var environment in environments)
                {
                    Ctx.SoftwareComponentEnvironments.Add(new SoftwareComponentEnvironment
                    {
                        SoftwareComponentId = sidDict[key].Id,
                        EnvironmentId = environment
                    });
                }
                
            }

            await Ctx.SaveChangesAsync();
            
            return id;
        }
    }
}