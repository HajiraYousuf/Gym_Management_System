using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be at least 3 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm your password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the terms")]
        public bool AgreeToTerms { get; set; }
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Trainer = "Trainer";
        public const string Receptionist = "Receptionist";
        public const string Member = "Member";
        public const string Guest = "Guest"; // Marka uu is-diiwaangeliyo

        public static string DashboardController(string? role) => role switch
        {
            Trainer => "Trainer",
            Receptionist => "Receptionist",
            Member => "Member",
            Admin => "Admin",
            Guest => "Guest",
            _ => "Guest"
        };

        public static string DashboardAction(string? role) => role switch
        {
            Admin => "Dashboard",
            Trainer => "Dashboard",
            Receptionist => "Dashboard",
            Member => "Dashboard",
            Guest => "HomePage", // Bogga uu ku sugayo in la ogolaado
            _ => "HomePage"
        };
    }
}