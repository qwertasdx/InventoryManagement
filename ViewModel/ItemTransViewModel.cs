using InventoryManagement.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModel
{
    public class ItemTransViewModel
    {
        public List<ItemTrans> Trans { get; set; }

        public List<SelectListItem> TypeList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "全部", Value=""},
            new SelectListItem { Text = "入貨", Value="in" },
            new SelectListItem { Text = "出貨", Value="out" }
        };

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
