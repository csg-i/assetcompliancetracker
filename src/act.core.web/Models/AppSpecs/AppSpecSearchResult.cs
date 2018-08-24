namespace act.core.web.Models.AppSpecs
{
    public class AppSpecSearchResult:ISpecSearchResult
    {
        public long Id { get; }
        public long OwnerId { get; }
        public string Name { get; }
        public string Owner { get; }
        public string OsSpecName { get; }
        public Counts Counts { get; }
        public long? LoggedInEmployeeId { get; set; }


        public AppSpecSearchResult(long id, string name, long ownerId, string owner, string osSpecName, Counts counts)
        {
            Id = id;
            OwnerId = ownerId;
            Name = name;
            Owner = owner;
            OsSpecName = osSpecName;
            Counts = counts;
        }
    }
}