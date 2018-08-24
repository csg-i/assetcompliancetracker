namespace act.core.web.Models.ScoreCard
{
    public class ExecutiveScoreCardScreen
    {
        public long EmployeeId { get; }
        public string Name { get; }

        public ExecutiveScoreCardScreen(long employeeId, string name)
        {
            EmployeeId = employeeId;
            Name = name;
        }
    }
}