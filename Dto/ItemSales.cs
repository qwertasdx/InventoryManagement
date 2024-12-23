namespace InventoryManagement.Dto
{
    public class ItemSales
    {
        public string ItemCode { get; set; } 
        public string ItemName { get; set; } //商品名稱
        public string Month { get; set; } 
        public int OutQty { get; set; }   //1-12月份出貨數量
    }
}
