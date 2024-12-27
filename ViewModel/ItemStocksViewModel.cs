using InventoryManagement.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModel
{
    public class ItemStocksViewModel : IPaginatable<ItemStocks>
    {
        public List<ItemStocks> Products { get; set; } = new List<ItemStocks>();

        // 實作 IPaginatable<T>
        public List<ItemStocks> Items
        {
            get => Products; 
            set => Products = value; // 將 Items 的變更回寫到 Products
        }

        public List<SelectListItem> StatusList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "啟用", Value = "10" },
            new SelectListItem { Text = "停用", Value = "20" }
        };

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
