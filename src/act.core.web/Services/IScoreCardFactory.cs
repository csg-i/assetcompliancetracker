using System.Threading.Tasks;
using act.core.web.Models.ScoreCard;

namespace act.core.web.Services
{
    public interface IScoreCardFactory
    {
        Task<OwnerScoreCard> GetOwnerScoreCard(long employeeId);
        Task<ExecutiveScoreCard> GetExecutiveScoreCard(long employeeId);
        Task<ProductScoreCard> GetProductScoreCard(string productCode);
        Task<DirectorScoreCard> GetDirectorScoreCard();
        Task<string[]> GetProductCodes();
        Task<PlatformScoreCard> GetPlatformScoreCard();
    }
}