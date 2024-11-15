using InventoryManagement.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.ViewModel
{
    public class ItemBasicSearchViewModel
    {
        [Required]

        public List<ItemBasics> News { get; set; }

        public List<SelectListItem> SpecList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "短袖", Value="10" },
            new SelectListItem { Text = "長袖", Value="11" },
            new SelectListItem { Text = "背心", Value="12" },
            new SelectListItem { Text = "短褲", Value="20" },
            new SelectListItem { Text = "長褲", Value="21" },
            new SelectListItem { Text = "包包", Value="30" },
            new SelectListItem { Text = "飾品", Value="31" },
        };
        public List<SelectListItem> StatusList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "啟用", Value="10" },
            new SelectListItem { Text = "停用", Value="20" },
            new SelectListItem { Text = "缺貨", Value="30" },
        };
    }
}
