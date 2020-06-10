using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using act.core.web.Models.Packages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace act.core.web.Services
{
    public interface ISoftwareComponentFactory
    {
        Task<Package[]> GetSoftwareComponents(JustificationTypeConstant type, long specId);

        Task<long?> AddSoftwareComponent(JustificationTypeConstant type, long specId, string name, string description,
            bool nonCore, PciScopeConstant[] pciScope, int[] environmentIds);

        Task<string[]> BulkAddSoftwareComponents(JustificationTypeConstant type, long specId,
            string[] names, string[] descriptions);

        Task<string[]> BulkAddSoftwareComponentsWithJustifications(JustificationTypeConstant type, long specId,
            string[] names, string[] justifications);

        Task RemoveSoftwareComponentDuplicates(JustificationTypeConstant type, long specId);

        Task AssignJustification(long id, long? justificationId);
        Task DeleteSoftwareComponent(long id);
        bool IsValidName(string name);
        Task<Package> GetSoftwareComponent(long id);

        Task SaveSoftwareComponent(long id, string name, string description, bool nonCore, PciScopeConstant[] pciScope,
            int[] environmentIds);

    }

    internal class SoftwareComponentFactory : ISoftwareComponentFactory
    {

        private readonly ActDbContext _ctx;
        private readonly ILogger _logger;

        public SoftwareComponentFactory(ActDbContext ctx, ILoggerFactory logger)
        {
            _ctx = ctx;
            _logger = logger.CreateLogger<SoftwareComponentFactory>();
        }

        public async Task<Package[]> GetSoftwareComponents(JustificationTypeConstant type, long specId)
        {
            var scs = await _ctx
                .SoftwareComponents.AsNoTracking()
                .ForBuildSpec(specId)
                .OfJustificationType(type)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.JustificationId,
                    p.NonCore,
                    p.BuildSpecification.BuildSpecificationType,
                    p.JustificationType,
                    p.PciScope,
                    EnvironmentIds = p.SoftwareComponentEnvironments.Select(q => q.EnvironmentId).ToArray(),
                    Envrionments = p.SoftwareComponentEnvironments.Select(q => q.Environment.Name)
                })
                .OrderBy(p => p.Name)
                .ToArrayAsync();//YANK FROM DB

            return scs.Select(p => new Package(p.BuildSpecificationType, p.JustificationType, p.Id, p.Name, p.Description,
                p.NonCore, p.JustificationId, p.PciScope, p.EnvironmentIds, string.Join("/", p.Envrionments))).ToArray();
        }

        public async Task<long?> AddSoftwareComponent(JustificationTypeConstant type, long specId, string name,
            string description, bool nonCore, PciScopeConstant[] pciScope, int[] environments)
        {
            if (!IsValidName(name))
                throw new ArgumentException($@"The {type} name is not valid, it must not be empty whitespace.",
                    nameof(name));
            var parentId = await _ctx.BuildSpecifications.Where(p => p.Id == specId).Select(p => p.ParentId)
                .FirstOrDefaultAsync();
            var ids = new List<long> {specId};
            if (parentId.HasValue)
                ids.Add(parentId.Value);

            if (_ctx.SoftwareComponents.Any(p =>
                ids.Contains(p.BuildSpecificationId) && p.Name == name && type == p.JustificationType))
                return null;

            var @new = _ctx.SoftwareComponents.Add(new SoftwareComponent
            {
                JustificationType = type,
                BuildSpecificationId = specId,
                Name = name,
                Description = description,
                NonCore = nonCore,
                PciScope = pciScope == null || pciScope.Length == 0 || pciScope.Length == 3 ? null : pciScope.ConvertToFlag()
            }).Entity;
            
            await _ctx.SaveChangesAsync();

            var environmentCount = await _ctx.Environments.CountAsync();

            if (environments != null && environmentCount != environments.Length)
            {
                foreach (var environment in environments)
                {
                    _ctx.SoftwareComponentEnvironments.Add(new SoftwareComponentEnvironment
                    {
                        EnvironmentId = environment,
                        SoftwareComponentId = @new.Id
                    });
                }
            }

            await _ctx.SaveChangesAsync();

            return @new.Id;
        }

        public async Task<string[]> BulkAddSoftwareComponents(JustificationTypeConstant type, long specId,
            string[] names, string[] descriptions)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            if (!names.All(IsValidName))
                throw new ArgumentException(
                    $@"At least one of the {type} names is not valid, it must not be empty whitespace.", nameof(names));

            if (descriptions != null && descriptions.Length > 0 && descriptions.Length != names.Length)
                throw new ArgumentException(
                    $@"At least one of the {type} descriptions the number of names does not match the number of descriptions",
                    nameof(descriptions));
            if (descriptions == null || descriptions.Length == 0)
                descriptions = new string[names.Length];

            var all = names.Select((name, index) => new SoftwareComponent
            {
                JustificationType = type,
                BuildSpecificationId = specId,
                Name = name,
                Description = descriptions[index]
            }).ToList();

            var notAdded = new List<string>();
            if (all.Count > 0)
            {
                var parentId = await _ctx.BuildSpecifications.Where(p => p.Id == specId).Select(p => p.ParentId)
                    .FirstOrDefaultAsync();
                var ids = new List<long> {specId};
                if (parentId.HasValue)
                    ids.Add(parentId.Value);


                all.ForEach(c =>
                {
                    if (_ctx.SoftwareComponents.Any(p =>
                        ids.Contains(p.BuildSpecificationId) && p.Name == c.Name &&
                        p.JustificationType == c.JustificationType))
                        notAdded.Add(c.Name);
                    else
                        _ctx.SoftwareComponents.Add(c);
                });
                if (all.Count != notAdded.Count)
                    await _ctx.SaveChangesAsync();
            }

            return notAdded.ToArray();
        }

        public async Task<string[]> BulkAddSoftwareComponentsWithJustifications(JustificationTypeConstant type,
            long specId, string[] names,
            string[] justifications)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));

            if (justifications == null)
                throw new ArgumentNullException(nameof(justifications));

            if (!names.All(IsValidName))
                throw new ArgumentException(
                    $@"At least one of the {type} names is not valid, it must not be empty whitespace.", nameof(names));

            if (justifications.Length != names.Length)
                throw new ArgumentException($@"At least one of the {type} names does not have a justification",
                    nameof(justifications));

            var all = names.Select((name, index) => new
            {
                Index = index,
                sc = new SoftwareComponent
                {
                    JustificationType = type,
                    BuildSpecificationId = specId,
                    Name = name
                }
            }).ToList();

            var notAdded = new List<string>();
            if (all.Count > 0)
            {
                var parentId = await _ctx.BuildSpecifications.Where(p => p.Id == specId).Select(p => p.ParentId)
                    .FirstOrDefaultAsync();
                var ids = new List<long> {specId};
                if (parentId.HasValue)
                    ids.Add(parentId.Value);


                all.ForEach(c =>
                {
                    if (_ctx.SoftwareComponents.Any(p =>
                        ids.Contains(p.BuildSpecificationId) && p.Name == c.sc.Name &&
                        p.JustificationType == c.sc.JustificationType))
                    {
                        notAdded.Add(c.sc.Name);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(justifications[c.Index]))
                        {
                            var justification = _ctx.Justifications.Add(new Justification
                            {
                                BuildSpecificationId = specId,
                                JustificationType = type,
                                JustificationText = justifications[c.Index]
                            }).Entity;
                            c.sc.Justification = justification;
                        }

                        _ctx.SoftwareComponents.Add(c.sc);
                    }
                });
                if (all.Count != notAdded.Count)
                    await _ctx.SaveChangesAsync();
            }

            return notAdded.ToArray();
        }

        public async Task RemoveSoftwareComponentDuplicates(JustificationTypeConstant type, long specId)
        {
            var parentId = await _ctx.BuildSpecifications.Where(p => p.Id == specId).Select(p => p.ParentId)
                .FirstOrDefaultAsync();
            if (parentId.HasValue)
            {
                var parents = _ctx.SoftwareComponents.ForBuildSpec(parentId.Value).OfJustificationType(type)
                    .Select(p => p.Name.ToLower()).ToArray();
                var mine = _ctx.SoftwareComponents.ForBuildSpec(specId).OfJustificationType(type)
                    .Where(p => parents.Contains(p.Name.ToLower()));
                foreach (var c in mine) _ctx.SoftwareComponents.Remove(c);

                await _ctx.SaveChangesAsync();
            }
        }

        public async Task AssignJustification(long id, long? justificationId)
        {
            var toUpdate = await _ctx.SoftwareComponents.ById(id);
            if (toUpdate == null)
                throw new ArgumentException(@"A software component with the id {id} was not found.", nameof(id));

            if (justificationId.HasValue)
            {
                if (await _ctx.Justifications
                    .ForBuildSpec(toUpdate.BuildSpecificationId)
                    .ExistsById(justificationId.Value)
                )
                    toUpdate.JustificationId = justificationId.Value;
                else
                    throw new ArgumentException(
                        $@"The justification with id {
                                justificationId
                            } is not part of the same spec as the software component, so it cannot be assigned.",
                        nameof(justificationId));
            }
            else
            {
                toUpdate.JustificationId = null;
            }

            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteSoftwareComponent(long id)
        {
            var toDelete = await _ctx.SoftwareComponents.ById(id);
            if (toDelete != null)
            {
                _ctx.SoftwareComponents.Remove(toDelete);
                await _ctx.SaveChangesAsync();
                _logger.LogInformation($"Successfully deleted package {toDelete.Name} from spec {toDelete.BuildSpecificationId}");
            }
        }

        public async Task SaveSoftwareComponent(long id, string name, string description, bool nonCore,
            PciScopeConstant[] pciScope, int[] environments)
        {
            if (!IsValidName(name))
                throw new ArgumentException(@"Name is not valid, it must not be empty whitespace.", nameof(name));
            var toUpdate = await _ctx.SoftwareComponents.Include(p=>p.SoftwareComponentEnvironments).ById(id);
            if (toUpdate == null)
                throw new ArgumentException($@"Could not find a software component with the id {id}", nameof(id));
            
            toUpdate.Name = name;
            toUpdate.Description = description;
            toUpdate.NonCore = nonCore;
            toUpdate.PciScope = pciScope == null || pciScope.Length == 0 || pciScope.Length == 3 ? null : pciScope.ConvertToFlag();

            var environmentCount = await _ctx.Environments.CountAsync();

            if (environments == null || environments.Length == 0 || environments.Length == environmentCount)
            {
                toUpdate.SoftwareComponentEnvironments.Clear();
            }
            else
            {
                var toRemove = toUpdate.SoftwareComponentEnvironments.Where(p => !environments.Contains(p.EnvironmentId)).ToArray();
                foreach (var remove in toRemove)
                {
                    toUpdate.SoftwareComponentEnvironments.Remove(remove);
                }

                var current = toUpdate.SoftwareComponentEnvironments.Select(p => p.EnvironmentId).ToArray();
                var toAdd = environments.Except(current);

                foreach (var add in toAdd)
                {
                    toUpdate.SoftwareComponentEnvironments.Add(new SoftwareComponentEnvironment
                    {
                        SoftwareComponentId = id,
                        EnvironmentId = add
                    });                    
                }
            }
            
            await _ctx.SaveChangesAsync();
        }
      
        public bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name);
        }

        public async Task<Package> GetSoftwareComponent(long id)
        {
            var toRet = await _ctx.SoftwareComponents.AsNoTracking().Include(p => p.BuildSpecification).ById(id);
            var envQuery = _ctx.SoftwareComponentEnvironments.Include(p=>p.Environment).BySoftwareComponent(id);
            if (toRet != null)
            {
                var envIds = await envQuery.Select(p => p.EnvironmentId).Distinct().ToArrayAsync();
                var desc = envQuery.GetEnvironmentNames();
               
                return new Package(toRet.BuildSpecification.BuildSpecificationType, toRet.JustificationType, id,
                    toRet.Name, toRet.Description, toRet.NonCore, toRet.JustificationId, toRet.PciScope, envIds, desc);
            }

            return null;
        }
    }
}