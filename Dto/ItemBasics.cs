﻿using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Dto
{
    public class ItemBasics
    {
        [Key]
        public string ItemCode { get; set; } = null!;

        public string ItemName { get; set; } = null!;

        public string Spec { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string EmployeeName { get; set; } = null!;

        public string StatusName { get; set; } = null!;

        public string SpecName { get; set; } = null!;

        public byte[] Img { get; set; }

        public DateTime SystemTime { get; set; }

        public int TotalQty { get; set; }
    }
}
