using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Dto
{
    public class ItemStocks
    {
        [Display(Name = "商品名稱")]
        public string ItemName { get; set; } = null!;

        [Display(Name = "商品貨號")]
        public string ItemCode { get; set; } = null!;

        [Display(Name = "單位")]
        public string Unit { get; set; } = null!;

        public int ?SafeQty { get; set; }

        [Display(Name = "總數量")]
        public int TotalQty { get; set; }

        public string Status { get; set; } = null!; 

        [Display(Name = "狀態")]
        public string StatusName { get; set; } = null!;

        public string EmployeeId { get; set; } = null!;
        public string ?EmployeeName { get; set; } = null!;
    }
}
