using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.Models
{
    public class ActivityLog
    {

        public int Id { get; set; }



        [Required]
        public string Description { get; set; }



        public DateTime Date { get; set; }



    }
}