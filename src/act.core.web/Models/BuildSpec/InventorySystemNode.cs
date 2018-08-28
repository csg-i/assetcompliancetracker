namespace act.core.web.Models.BuildSpec
{
    public class InventorySystemNode
    {
        public long InventoryItemId { get; }
        public string Fqdn { get; }

        public InventorySystemNode(long inventoryItemId, string fqdn)
        {
            InventoryItemId = inventoryItemId;
            Fqdn = fqdn;
        }
    }
}