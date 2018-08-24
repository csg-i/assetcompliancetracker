using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class SpecByOwner
    {
        public SpecByOwner(string owner, string samAccountName)
        {
            Owner = owner;
            SamAccountName = samAccountName;
        }

        public string Owner { get; }
        public string SamAccountName { get; }
        public int AppSpecCount { get; private set; }
        public int OsSpecCount { get; private set; }

        internal void SetCount(BuildSpecificationTypeConstant type, int count)
        {
            switch (type)
            {
                case BuildSpecificationTypeConstant.Application:
                    AppSpecCount = count;
                    break;
                case BuildSpecificationTypeConstant.OperatingSystem:
                    OsSpecCount = count;
                    break;                
            }
        }
    }
}