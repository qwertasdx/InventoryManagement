using InventoryManagement.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModel
{
    public class ItemStocksViewModel
    {
        public List<ItemStocks> Products { get; set; }

        public List<SelectListItem> StatusList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "啟用", Value="10" },
            new SelectListItem { Text = "停用", Value="20" }
        };

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
