using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GymManagementSystem.Models
{
    public class ClassSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Fadlan geli magaca fasalka.")]
        public string ClassName { get; set; }

        [Required(ErrorMessage = "Fadlan dooro tababaraha.")]
        public string TrainerName { get; set; }

        [Required(ErrorMessage = "Fadlan dooro taariikhda.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Fadlan geli waqtiga bilaabashada.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Fadlan geli waqtiga dhamaadka.")]
        public TimeSpan EndTime { get; set; }

        public string Location { get; set; } = "Weight Training Area";

        [Required]
        public int Capacity { get; set; } = 15;

        public string Status { get; set; } = "Scheduled";
    }

    public class AttendanceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MemberId { get; set; } = string.Empty;

        [ForeignKey(nameof(MemberId))]
        public UserProfile? Member { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Present";

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? TrainerName { get; set; }
    }

    public class Exercise
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Exercise name is required")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Primary muscle is required")]
        [StringLength(50)]
        public string PrimaryMuscle { get; set; } = string.Empty;

        [StringLength(150)]
        public string SecondaryMuscle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Equipment is required")]
        [StringLength(50)]
        public string Equipment { get; set; } = string.Empty;

        [StringLength(20)]
        public string Difficulty { get; set; } = "Beginner";

        [StringLength(300)]
        public string VideoUrl { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(300)]
        public string ThumbnailPath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string DifficultyLevel
        {
            get => Difficulty;
            set => Difficulty = value;
        }

        [NotMapped]
        public string ThumbnailUrl
        {
            get => ThumbnailPath;
            set => ThumbnailPath = value;
        }

        [NotMapped]
        public IFormFile? Thumbnail { get; set; }
    }

    public class GymClass
    {
        [Key]
        [Column("ClassID")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ClassName")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("Category")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Column("DurationMinutes")]
        public int DurationMinutes { get; set; }

        [Required]
        [Column("Capacity")]
        public int Capacity { get; set; }

        [StringLength(100)]
        [Column("TrainerName")]
        public string TrainerName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("Days")]
        public string Days { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("TimeSlot")]
        public string TimeSlot { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("Difficulty")]
        public string Difficulty { get; set; } = "All Levels";

        [StringLength(300)]
        [Column("Equipment")]
        public string Equipment { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("Icon")]
        public string IconName { get; set; } = "dumbbell";

        [StringLength(500)]
        [Column("ImageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Active";
    }

    public class AttendanceFilterViewModel
    {
        public DateTime? Date { get; set; }
        public string? Search { get; set; }
        public string Status { get; set; } = "All";
        public List<AttendanceRecord> Records { get; set; } = new();
    }

    public class GalleryImage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title-ka waa khasab")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Category-ga waa khasab")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Image URL-ku waa khasab")]
        public string ImageUrl { get; set; }

        public bool IsVisible { get; set; } = true;
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }

    public class ClassFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Class name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 300)]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 500)]
        public int Capacity { get; set; }

        public string Difficulty { get; set; } = "All Levels";
        public string Equipment { get; set; } = string.Empty;
        public string IconName { get; set; } = "dumbbell";
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class MembershipPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Required(ErrorMessage = "Plan name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Short code is required")]
        [StringLength(20)]
        public string ShortCode { get; set; }

        [Required(ErrorMessage = "Membership type is required")]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        public int DurationDays { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }

        public int? Installments { get; set; } = 0;

        [Range(0, 10000)]
        public decimal SignupFee { get; set; } = 0;

        public string? Description { get; set; } = string.Empty;
        public string? Features { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}