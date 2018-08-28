using act.core.data;

namespace act.core.web.Models.Packages
{
    public class NewBulkPackages:NewPackage
    {
        public string HelpTextPartial => $"{FriendlyName}HelpPartial";
        public string FriendlyNamePlural => $"{FriendlyName}s";
        public string FriendlyNameLower => FriendlyName.ToLower();
        public NewBulkPackages(JustificationTypeConstant packageType, BuildSpecificationTypeConstant buildSpecificationType, long specId):base(packageType, buildSpecificationType, specId)
        {
            
        }
    }
}