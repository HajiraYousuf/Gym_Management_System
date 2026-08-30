using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementSystem.Models
{
    public class Visitor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string VisitType { get; set; }

        public string? InvitedByMember { get; set; }
        public string? Purpose { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        public string Status { get; set; } = "Inside";
        public string? AssignedTrainer { get; set; }
    }

    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string MemberName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PaymentFor { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Method { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class GymReservation
    {
        [Key]
        public int ReservationID { get; set; }

        [Required]
        [StringLength(50)]
        public string ReservationNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string MemberName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TrainerName { get; set; } = string.Empty;

        public DateTime ReservationDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
    }
}