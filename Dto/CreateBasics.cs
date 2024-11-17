namespace InventoryManagement.Dto
{
    public class CreateBasics
    {
        //public string ItemCode { get; set; } = null!;

        public string ItemName { get; set; } = null!;

        public string Spec { get; set; } = null!;

        public string Status { get; set; } = null!;

        public byte[] Img { get; set; } 
    }
}
