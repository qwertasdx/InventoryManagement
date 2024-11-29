namespace InventoryManagement.Dto
{
    public class CreateBasics
    {
        public string ItemCode { get; set; } = null!;

        public string ItemName { get; set; } = null!;

        public string Spec { get; set; } = null!;

        public string Status { get; set; } = null!;

        public byte[] Img { get; set; }

        //新增到 Table ItemStock
        public string Unit { get; set; } = null!;
    }
}
