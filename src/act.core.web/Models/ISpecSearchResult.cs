namespace act.core.web.Models
{
    public interface ISpecSearchResult
    {
        long Id { get; }
        string Name { get; }
        string Owner { get; }
        long OwnerId { get; }
        Counts Counts { get; }
        long? LoggedInEmployeeId { get; set; }

    }
}