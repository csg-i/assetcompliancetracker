namespace act.core.web.Models.BuildSpec
{
    public class GamNode
    {
        public long InventoryItemId { get; }
        public string Fqdn { get; }

        public GamNode(long inventoryItemId, string fqdn)
        {
            InventoryItemId = inventoryItemId;
            Fqdn = fqdn;
        }
    }
}