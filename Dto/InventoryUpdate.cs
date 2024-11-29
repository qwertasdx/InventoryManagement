namespace InventoryManagement.Dto
{
    public class InventoryUpdate
    {
        public string ItemCode { get; set; }

        public string ItemName { get; set; }

        public int InventoryQty { get; set; }

        public int DiffQty { get; set; }

        public string Reason { get; set; }
    }
}
