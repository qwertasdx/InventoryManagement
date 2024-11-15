using System;
using System.Collections.Generic;

namespace InventoryManagement.Models;

public partial class NewsFiles
{
    public Guid NewsFilesId { get; set; }

    public Guid NewsId { get; set; }

    public string Name { get; set; } = null!;

    public string Path { get; set; } = null!;

    public string Extension { get; set; } = null!;
}
