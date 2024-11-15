using System;
using System.Collections.Generic;

namespace InventoryManagement.Models;

public partial class ItemBasic
{
    public string ItemCode { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string Spec { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string SystemUser { get; set; } = null!;

    public DateTime SystemTime { get; set; }
}
