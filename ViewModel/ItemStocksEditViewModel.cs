using InventoryManagement.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModel
{
    public class ItemStocksEditViewModel
    {
        public List<ItemStocksEdit> Products { get; set; }

        public List<SelectListItem> StatusList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "啟用", Value="10" },
            new SelectListItem { Text = "停用", Value="20" }
        };

        public List<SelectListItem> UnitList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "件", Value="件" },
            new SelectListItem { Text = "個", Value="個" },
            new SelectListItem { Text = "雙", Value="雙" },
        };
    }
}
