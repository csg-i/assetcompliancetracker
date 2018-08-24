using System.Collections.Generic;
using System.Linq;

namespace act.core.web.Models.ScoreCard
{
    public interface IDirectorScoreCard
    {
        long? DirectorId { get; }
        string Director { get; }
        long? OwnerId { get; }
        string Owner { get; }
        int SpecCount { get; }
        ScoreCardPciCount AssignedNodes { get; }
        ScoreCardPciCount UnassignedNodes { get; }
        ScoreCardPciCount PassingNodes { get; }
        ScoreCardPciCount FailingNodes { get; }
        ScoreCardPciCount NotReportingNodes { get; }
        ScoreCardPciCount OutOfChefScopeNodes { get; }
        ScoreCardPciCount AllNodes { get; }
    }

    public class DirectorScoreCard:List<DirectorScoreCardRow>,IDirectorScoreCard
    {
        public DirectorScoreCard(DirectorScoreCardRow[] rows) : base(rows)
        {
            SpecCount = rows.Sum(p => p.SpecCount);
            AllNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.AllNodes);
            AssignedNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.AssignedNodes);
            PassingNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.PassingNodes);
            FailingNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.FailingNodes);
            NotReportingNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.NotReportingNodes);
            OutOfChefScopeNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.OutOfChefScopeNodes);
            UnassignedNodes = rows.Aggregate(new ScoreCardPciCount(), (s, p) => s + p.UnassignedNodes);
        }

        public bool Empty => Count == 0;

        long? IDirectorScoreCard.DirectorId => null;

        string IDirectorScoreCard.Director => null;

        long? IDirectorScoreCard.OwnerId => null;

        string IDirectorScoreCard.Owner => null;

        public int SpecCount { get; }
        public ScoreCardPciCount AssignedNodes { get; }
        public ScoreCardPciCount UnassignedNodes { get; }
        public ScoreCardPciCount PassingNodes { get; }
        public ScoreCardPciCount FailingNodes { get; }
        public ScoreCardPciCount NotReportingNodes { get; }
        public ScoreCardPciCount OutOfChefScopeNodes { get; }
        public ScoreCardPciCount AllNodes { get; }
    }

    public class DirectorScoreCardRow : IDirectorScoreCard
    {
        public DirectorScoreCardRow(long? ownerId, string owner, long? directorId, string director, int specCount, ScoreCardPciCount assignedNodes, ScoreCardPciCount passingNodes, ScoreCardPciCount failingNodes, ScoreCardPciCount notReportingNodes, ScoreCardPciCount outOfChefScopeNodes, ScoreCardPciCount allNodes)
        {
            DirectorId = directorId;
            Director = director;
            OwnerId = ownerId;
            Owner = owner;
            SpecCount = specCount;
            AssignedNodes = assignedNodes;
            PassingNodes = passingNodes;
            FailingNodes = failingNodes;
            NotReportingNodes = notReportingNodes;
            OutOfChefScopeNodes = outOfChefScopeNodes;
            AllNodes = allNodes;
            UnassignedNodes = allNodes - assignedNodes;
        }

        public long? DirectorId { get; }
        public string Director { get; }
        public long? OwnerId { get; }
        public string Owner { get; }
        public int SpecCount { get; }
        public ScoreCardPciCount AssignedNodes { get; }
        public ScoreCardPciCount UnassignedNodes { get; }
        public ScoreCardPciCount PassingNodes { get; }
        public ScoreCardPciCount FailingNodes { get; }
        public ScoreCardPciCount NotReportingNodes { get; }
        public ScoreCardPciCount OutOfChefScopeNodes { get; }
        public ScoreCardPciCount AllNodes { get; }
    }
}