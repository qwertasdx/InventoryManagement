namespace InventoryManagement.Dto
{
    public class ItemBasicsEdit
    {
        public string ItemCode { get; set; } = null!;

        public string ItemName { get; set; } = null!;

        public string Spec { get; set; } = null!;
        public string SpecName { get; set; } = null!;

        public string Status { get; set; } = null!;

        public byte [] Img { get; set; } = null!;

        public IFormFile? NewImg { get; set; } 


    }
}
