using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }


        [Required]
        public int MemberId { get; set; }

        public Member? Member { get; set; }


        [Required]
        public int MembershipId { get; set; }

        public Membership? Membership { get; set; }


        [Required]
        [Range(1, 100000)]
        public decimal Amount { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }


        [Required]
        public string PaymentMethod { get; set; } = string.Empty;


        [Required]
        public string Status { get; set; } = "Paid";
    }
}