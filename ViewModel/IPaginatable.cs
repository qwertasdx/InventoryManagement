namespace InventoryManagement.ViewModel
{
    public interface IPaginatable<T>
    {
        List<T> Items { get; set; }
        int CurrentPage { get; set; }
        int TotalPages { get; set; }
    }
}
