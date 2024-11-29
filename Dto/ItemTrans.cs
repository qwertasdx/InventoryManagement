namespace InventoryManagement.Dto
{
    public class ItemTrans
    {
        public string ItemCode { get; set; } = null!;

        public string ItemName { get; set; } = null!;

        public string Type { get; set; } = null!;
        public string TypeName { get; set; } = null!;

        public string Unit { get; set; } = null!;

        public int TransQty { get; set; }
  
        public string EmployeeName { get; set; } = null!;
        public string SystemUser { get; set; } = null!;

        public DateTime SystemTime { get; set; }

        // Trans2
        public string? Reason { get; set; } = null!;
    }
}
