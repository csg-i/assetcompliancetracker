using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class InventorySystemNode
    {
        public long InventoryItemId { get; }
        public string Fqdn { get; }
        public string Owner { get; }
        public string RemedyGroupEmail { get; }

        public InventorySystemNode(long inventoryItemId, string fqdn, string owner, string remedyGroupEmail)
        {
            InventoryItemId = inventoryItemId;
            Fqdn = fqdn;
            Owner = owner;
            RemedyGroupEmail = remedyGroupEmail;
        }

    }
}