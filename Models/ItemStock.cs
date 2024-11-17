using System;
using System.Collections.Generic;

namespace InventoryManagement.Models;

public partial class ItemStock
{
    public string ItemCode { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public int SafeQty { get; set; }

    public int TotalQty { get; set; }

    public string Status { get; set; } = null!;

    public string SystemUser { get; set; } = null!;

    public DateTime SystemTime { get; set; }
}
