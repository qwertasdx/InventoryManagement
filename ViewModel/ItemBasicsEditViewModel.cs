using InventoryManagement.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModel
{
    public class ItemBasicsEditViewModel
    {
        public ItemBasicsEdit News { get; set; }

        public string imageBase64 { get; set; }

        public List<SelectListItem> StatusList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "啟用", Value="10" },
            new SelectListItem { Text = "停用", Value="20" },
            new SelectListItem { Text = "缺貨", Value="30" },
        };
    }
}
