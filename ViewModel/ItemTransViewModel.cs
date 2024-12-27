using InventoryManagement.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModel
{
    public class ItemTransViewModel : IPaginatable<ItemTrans>
    {
        public List<ItemTrans> Trans { get; set; } = new List<ItemTrans>();

        // 實作 IPaginatable<T>
        public List<ItemTrans> Items
        {
            get => Trans;
            set => Trans = value; // 將 Items 的變更回寫到 Trans
        }

        public List<SelectListItem> TypeList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "全部", Value=""},
            new SelectListItem { Text = "入貨", Value="in" },
            new SelectListItem { Text = "出貨", Value="out" }
        };

        public List<SelectListItem> TypeList2 { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "全部", Value=""},
            new SelectListItem { Text = "盤盈", Value="in" },
            new SelectListItem { Text = "盤虧", Value="out" }
        };

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
