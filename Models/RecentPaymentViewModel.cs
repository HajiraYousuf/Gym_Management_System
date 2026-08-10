using System;

namespace GymManagementSystem.Models
{
    public class RecentPaymentViewModel
    {
        public string MemberName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
