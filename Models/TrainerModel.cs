using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementSystem.Models
{
    public class MemberProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MemberId { get; set; } // Wuxuu xiriir la leeyahay UserProfile.Id ama User.Id

        [ForeignKey(nameof(MemberId))]
        public UserProfile? Member { get; set; }

        public decimal? Weight { get; set; }
        public decimal? BodyFat { get; set; }
        public decimal? Chest { get; set; }
        public decimal? Waist { get; set; }
        public decimal? Arms { get; set; }

        public string? Goal { get; set; }
        public decimal? TargetWeight { get; set; }
        public string? Notes { get; set; }

        public DateTime CheckDate { get; set; } = DateTime.Now;
        public DateTime NextCheckDate { get; set; }
    }

    public class NutritionPlan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Plan name is required.")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string TargetGoal { get; set; }

        public int Calories { get; set; }
        public int DurationWeeks { get; set; }

        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fats { get; set; }

        public string? DailyMealsJson { get; set; }
        public string? Notes { get; set; }

        public string? AssignedMemberId { get; set; }

        [ForeignKey(nameof(AssignedMemberId))]
        public UserProfile? AssignedMember { get; set; }
        public int? TrainerId { get; set; }
        public virtual TrainerProfile? Trainer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class WorkoutPlan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Plan name is required.")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string TargetGoal { get; set; }

        [Required]
        public string DifficultyLevel { get; set; }

        public int DurationWeeks { get; set; } = 4;
        public int DaysPerWeek { get; set; } = 4;
        public int SessionDurationMinutes { get; set; } = 60;

        public string? ExercisesJson { get; set; }
        public string? Notes { get; set; }

        public string? AssignedMemberId { get; set; }

        [ForeignKey(nameof(AssignedMemberId))]
        public UserProfile? AssignedMember { get; set; }

        public int? TrainerId { get; set; }
        public virtual TrainerProfile? Trainer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}