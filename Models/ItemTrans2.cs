using System;
using System.Collections.Generic;

namespace InventoryManagement.Models;

public partial class ItemTrans2
{
    public int TransNo { get; set; }

    public string ItemCode { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public int TransQty { get; set; }

    public string Reason { get; set; } = null!;

    public string SystemUser { get; set; } = null!;

    public DateTime SystemTime { get; set; }
}
