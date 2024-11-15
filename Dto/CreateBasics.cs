namespace InventoryManagement.Dto
{
    public class CreateBasics
    {
        //public string ItemCode { get; set; } = null!;

        public string ItemName { get; set; } = null!;

        public string Spec { get; set; } 

        public string Unit { get; set; } = null!;

        public string Status { get; set; } 

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
