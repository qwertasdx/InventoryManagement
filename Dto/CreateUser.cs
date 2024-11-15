namespace InventoryManagement.Dto
{
    public class CreateUser
    {
        public string EmployeeId { get; set; } = null!;

        public string EmployeeName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
