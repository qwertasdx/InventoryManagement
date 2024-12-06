using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Dto
{
    public class Users
    {
        [Display(Name = "員工編號")]
        public string EmployeeId { get; set; } = null!;

        [Display(Name = "員工姓名")]
        public string EmployeeName { get; set; } = null!;

        [Display(Name = "帳號")]
        public string Account { get; set; } = null!;

        public string Role { get; set; } = null!;

        [Display(Name = "身分")]
        public string RoleName { get; set; } = null!;
    }
}
