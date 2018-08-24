namespace act.core.web.Models.ScoreCard
{
    public class OwnerScoreCardScreen
    {
        public long EmployeeId { get; }
        public string Name { get; }

        public OwnerScoreCardScreen(long employeeId, string name)
        {
            EmployeeId = employeeId;
            Name = name;
        }
    }
}