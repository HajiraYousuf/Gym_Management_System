using System.Collections.Generic;

namespace GymManagementSystem.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalMembers { get; set; }
        public int TotalTrainers { get; set; }
        public int ActiveMemberships { get; set; }
        public List<RecentPaymentViewModel> RecentPayments { get; set; }
    }
}
