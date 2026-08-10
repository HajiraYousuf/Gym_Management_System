using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.Models
{
    public class MembershipPlan
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Plan Name")]
        public string PlanName { get; set; }

        [Required]
        [Range(0, 999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 3650)]
        [Display(Name = "Duration in Days")]
        public int DurationInDays { get; set; }

        public ICollection<Membership> Memberships { get; set; }
            = new List<Membership>();
    }
}