using act.core.data;

namespace act.core.web.Models.OsSpecs
{
    public class OsSpecSearchResult : ISpecSearchResult
    {
        public long Id { get; }
        public string Name { get; }
        public string Owner { get; }
        public long OwnerId { get; }
        public PlatformConstant Platform { get; }
        public Counts Counts { get; }
        public long? LoggedInEmployeeId { get; set; }

        public OsSpecSearchResult(long id, string name, long ownerId, string owner, PlatformConstant platform, Counts counts)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            Owner = owner;
            Platform = platform;
            Counts = counts;
        }
    }
}