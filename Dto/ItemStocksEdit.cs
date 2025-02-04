﻿namespace InventoryManagement.Dto
{
    public class ItemStocksEdit
    {
        public string ItemName { get; set; } = null!;

        public string ItemCode { get; set; } = null!;

        public string Unit { get; set; } = null!;

        public int SafeQty { get; set; }

        public int TotalQty { get; set; }

        public string Status { get; set; } = null!;
    }
}
