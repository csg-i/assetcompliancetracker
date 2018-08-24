using System.Collections.Generic;
using System.Linq;

namespace act.core.web.Models.ScoreCard
{
    public interface IProductScoreCard
    {
        long? DirectorId { get; }
        string Director { get; }
        long? OwnerId { get; }
        string Owner { get; }
        string Code { get; }
        string Description { get; }
        int? FunctionId { get; }
        string Function { get; }
        int SpecCount { get; }
        ScoreCardPciCount AssignedNodes { get; }
        ScoreCardPciCount UnassignedNodes { get; }
        ScoreCardPciCount PassingNodes { get; }
        ScoreCardPciCount FailingNodes { get; }
        ScoreCardPciCount NotReportingNodes { get; }
        ScoreCardPciCount OutOfChefScopeNodes { get; }
        ScoreCardPciCount AllNodes { get; }
    }

    public class ProductScoreCard : List<ProductScoreCardRow>, IProductScoreCard
    {
        public string Code { get; }
        public string Description { get; }

        public ProductScoreCard(string code, string description, ProductScoreCardRow[] rows) : base(rows)
        {
            Code = code;
            Description = description;

            SpecCount = rows.Sum(p => p.SpecCount);
            AllNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.AllNodes);
            AssignedNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.AssignedNodes);
            PassingNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.PassingNodes);
            FailingNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.FailingNodes);
            NotReportingNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.NotReportingNodes);
            OutOfChefScopeNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.OutOfChefScopeNodes);
            UnassignedNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.UnassignedNodes);
        }

        int? IProductScoreCard.FunctionId => null;
        string IProductScoreCard.Function => null;
        long? IProductScoreCard.OwnerId => null;
        string IProductScoreCard.Owner => null;
        long? IProductScoreCard.DirectorId => null;
        string IProductScoreCard.Director => null;
        public int SpecCount { get; }
        public ScoreCardPciCount AllNodes { get; }
        public ScoreCardPciCount AssignedNodes { get; }
        public ScoreCardPciCount UnassignedNodes { get; }
        public ScoreCardPciCount PassingNodes { get; }
        public ScoreCardPciCount FailingNodes { get; }
        public ScoreCardPciCount NotReportingNodes { get; }
        public ScoreCardPciCount OutOfChefScopeNodes { get; }
        public bool Empty => Count == 0;
    }
}