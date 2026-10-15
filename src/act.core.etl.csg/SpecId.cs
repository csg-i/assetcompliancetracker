using act.core.data;

namespace act.core.etl.csg
{
    public class SpecId
    {
        public long Id { get; set; }
        public string Type => SpecTypeId == BuildSpecificationTypeConstant.Application ? "app" : "os";
        internal BuildSpecificationTypeConstant SpecTypeId { get; set; }
        internal string Name { get; set; }
        public string VersionDescription { get; set; }
    }
}