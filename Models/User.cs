using System;
using System.Collections.Generic;

namespace InventoryManagement.Models;

public partial class User
{
    public string EmployeeId { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public string Account { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;
}
