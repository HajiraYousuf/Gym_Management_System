namespace GymManagementSystem.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }
}