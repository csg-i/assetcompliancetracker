namespace act.core.data
{
    public abstract class LongId
    {
        public abstract long Id { get; set; }

        public abstract byte[] TimeStamp { get; set; }
    }
}