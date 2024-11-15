using System;
using System.Collections.Generic;

namespace InventoryManagement.Models;

public partial class News
{
    public Guid NewsId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public DateTime InsertDateTime { get; set; }

    public DateTime UpdateDateTime { get; set; }

    public string InsertEmployeeId { get; set; } = null!;

    public string UpdateEmployeeId { get; set; } = null!;

    public int Click { get; set; }

    public bool Enable { get; set; }

    public int DepartmentId { get; set; }
}
