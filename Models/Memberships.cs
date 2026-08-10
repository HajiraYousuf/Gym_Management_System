using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.Models
{
    public class Membership
    {

        public int Id { get; set; }



        public int MemberId { get; set; }


        public Member? Member { get; set; }




        public int MembershipPlanId { get; set; }


        public MembershipPlan? MembershipPlan { get; set; }





        public DateTime StartDate { get; set; }



        public DateTime EndDate { get; set; }



        public string? Status { get; set; }

    }
}