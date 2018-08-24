using System;
using System.Linq;
using System.Threading.Tasks;
using act.core.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Justification = act.core.web.Models.Justifications.Justification;

namespace act.core.web.Services
{
    public interface IJustificationFactory
    {
        Task<Justification[]> GetJustifications(JustificationTypeConstant type, long specId);
        Task<long> AddJustification(JustificationTypeConstant type, long specId, string text);
        Task UpdateJustification(long id, string text);
        Task DeleteJustification(long id);
        bool IsValidText(string text);
        Task<Justification> GetJustification(long id);
        Task ChangeColor(long id, string color);
    }

    internal class JustificationFactory : IJustificationFactory
    {
        private readonly ActDbContext _ctx;
        private readonly ILogger _logger;

        public JustificationFactory(ActDbContext ctx, ILoggerFactory loggerFactory)
        {
            _ctx = ctx;
            _logger = loggerFactory.CreateLogger<JustificationFactory>();
        }

        public async Task<Justification[]> GetJustifications(JustificationTypeConstant type, long specId)
        {
            {
                return (await _ctx
                        .Justifications.AsNoTracking()
                        .ForBuildSpec(specId)
                        .OfJustificationType(type)
                        .Select(p => new {p.Id, p.JustificationText, p.JustificationType, p.Color})
                        .ToArrayAsync()) //YANK FROM DB
                    .Select(p => new Justification(p.Id, p.JustificationText, p.JustificationType, p.Color))
                    .ToArray();
            }
        }

        public async Task<long> AddJustification(JustificationTypeConstant type, long specId, string text)
        {
            if (!IsValidText(text))
                throw new ArgumentException(@"The Text is not valid, it must not be empty whitespace.", nameof(text));
            {
                var @new = _ctx.Justifications.Add(new data.Justification
                {
                    JustificationType = type,
                    BuildSpecificationId = specId,
                    JustificationText = text
                }).Entity;
                await _ctx.SaveChangesAsync();
                return @new.Id;
            }
        }

        public bool IsValidText(string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        public async Task UpdateJustification(long id, string text)
        {
            if (!IsValidText(text))
                throw new ArgumentException(@"The Text is not valid, it must not be empty whitespace.", nameof(text));
            var toUpdate = await _ctx.Justifications.ById(id);
            if (toUpdate == null)
                throw new ArgumentException(@"A justification with the id {id} was not found.", nameof(id));
            toUpdate.JustificationText = text;
            await _ctx.SaveChangesAsync();
        }

        public async Task<Justification> GetJustification(long id)
        {
            var toRet = await _ctx.Justifications.AsNoTracking().ById(id);
            if (toRet != null)
                return new Justification(id, toRet.JustificationText, toRet.JustificationType, toRet.Color);

            return null;
        }

        public async Task ChangeColor(long id, string color)
        {
            var toUpdate = await _ctx.Justifications.ById(id);
            if (toUpdate == null)
                throw new ArgumentException(@"A software component with the id {id} was not found.", nameof(id));

            toUpdate.Color = color;
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteJustification(long id)
        {
            var toDelete = await _ctx.Justifications.Include(p => p.Ports).Include(p => p.SoftwareComponents).ById(id);

            if (toDelete != null)
            {
                toDelete.Ports.Clear();
                toDelete.SoftwareComponents.Clear();
                _ctx.Justifications.Remove(toDelete);
                await _ctx.SaveChangesAsync();
                _logger.LogInformation($"Succesfully deleted justification with id {id} from buildspec {toDelete.BuildSpecificationId}");
            }
        }
    }
}