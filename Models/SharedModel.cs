using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.Models
{
    public class SharedModel
    {
        public User CurrentUser { get; set; }
        public UserProfile Profile { get; set; }
        public List<User> Contacts { get; set; }
        public List<Message> Messages { get; set; }
        public User ActiveContact { get; set; }
        public List<Notification> Notifications { get; set; }

        public string LayoutPath => CurrentUser?.Role switch
        {
            "Member" => "~/Views/layouts/MemberMaster.cshtml",
            "Trainer" => "~/Views/layouts/TrainerMaster.cshtml",
            "Receptionist" => "~/Views/layouts/ReceptionMaster.cshtml",
            _ => "~/Views/layouts/AdminMaster.cshtml"
        };
    }

    public class User
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }
        public string Status { get; set; }
        public DateTime LastSeen { get; set; }

        public string Specialization { get; set; }
        public string MembershipType { get; set; }
        public string Shift { get; set; }
    }

    public class UserProfile
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Bio { get; set; }
        public string Avatar { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastLogin { get; set; }
        public string Password { get; set; }
        public string MembershipType { get; set; }
        public string Specialization { get; set; }
        public string Shift { get; set; }
    }

    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string TargetRole { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime Timestamp { get; set; }
    }
}