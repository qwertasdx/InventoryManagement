using InventoryManagement.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagement.ViewModel
{
    public class UsersViewModel
    {
        public Users Users { get; set; }

        public List<SelectListItem> RoleNameList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "一般人員", Value="employee" },
            new SelectListItem { Text = "管理人員", Value="admin" }
        };
    }
}
