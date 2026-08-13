using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GymManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GymManagementSystem.Controllers
{
    [Authorize]
    public class SharedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SharedController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1. CHAT / MESSAGES ACTIONS
        // =========================================================
        public IActionResult Messages(string userId = "member1", string activeContactId = null)
        {
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == userId) ?? _context.Users.FirstOrDefault();
            var allowedContacts = new List<User>();

            if (currentUser != null)
            {
                if (currentUser.Role == "Member")
                {
                    allowedContacts = _context.Users.Where(u => u.Role == "Trainer" || u.Role == "Admin" || u.Role == "Receptionist").ToList();
                }
                else if (currentUser.Role == "Trainer")
                {
                    allowedContacts = _context.Users.Where(u => u.Role == "Admin" || u.Role == "Receptionist" || u.Role == "Member").ToList();
                }
                else
                {
                    allowedContacts = _context.Users.Where(u => u.Id != currentUser.Id).ToList();
                }

            }

            var activeContact = allowedContacts.FirstOrDefault(u => u.Id == activeContactId) ?? allowedContacts.FirstOrDefault();

            var messages = new List<Message>();
            if (currentUser != null && activeContact != null)
            {
                messages = _context.Messages.Where(m =>
                    (m.SenderId == currentUser.Id && m.ReceiverId == activeContact.Id) ||
                    (m.SenderId == activeContact.Id && m.ReceiverId == currentUser.Id)
                ).OrderBy(m => m.Id).ToList();
            }

            var viewModel = new SharedModel
            {
                CurrentUser = currentUser,
                Contacts = allowedContacts,
                ActiveContact = activeContact,
                Messages = messages
            };

            return View("~/Views/Shared/Messages.cshtml", viewModel);
        }

        [HttpPost]
        public IActionResult SendMessage(string senderId, string receiverId, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var newMessage = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Text = text,
                    Timestamp = DateTime.Now
                };

                _context.Messages.Add(newMessage);
                _context.SaveChanges();
            }

            return RedirectToAction("Messages", new { userId = senderId, activeContactId = receiverId });
        }

        // =========================================================
        // 2. NOTIFICATIONS ACTIONS
        // =========================================================
        public IActionResult Notifications(string role = "Admin")
        {
            var roleNotifications = _context.Notifications
                .Where(n => n.TargetRole == role || n.TargetRole == "All")
                .OrderByDescending(n => n.Timestamp)
                .ToList();

            var viewModel = new SharedModel
            {
                CurrentUser = new User { Role = role },
                Notifications = roleNotifications
            };

            return View("~/Views/Shared/Notification.cshtml", viewModel);
        }

        [HttpPost]
        [HttpPost]
        [HttpPost]
        public IActionResult MarkAllAsRead(string role)
        {
            // 1. Hel dhammaan ogeysiisyada aan la akhriyin
            var unreadNotifs = _context.Notifications
                .Where(n => (n.TargetRole == role || n.TargetRole == "All") && !n.IsRead)
                .ToList();

            // 2. U beddel IsRead = true
            foreach (var notif in unreadNotifs)
            {
                notif.IsRead = true;
            }

            // 3. Save garee
            _context.SaveChanges();

            // 4. Dib ugu celi bogga
            return RedirectToAction("Notifications", new { role = role });
        }
        [HttpPost]
        [HttpPost]
        public IActionResult DismissNotification(int id, string role)
        {
            // Waxay raadineysaa ogeysiiska iyadoo la isticmaalayo ID-giisa oo way tirtiraysaa (Delete)
            var notif = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (notif != null)
            {
                _context.Notifications.Remove(notif);
                _context.SaveChanges();
            }

            return RedirectToAction("Notifications", new { role = role });
        }
        // =========================================================
        // 3. PROFILE ACTIONS
        // =========================================================
        public IActionResult Profile(string userId = "ADM-01")
        {
            var profile = _context.UserProfiles.FirstOrDefault(p => p.Id == userId) ?? _context.UserProfiles.FirstOrDefault(p => p.Id == "ADM-01") ?? _context.UserProfiles.FirstOrDefault();

            var viewModel = new SharedModel
            {
                CurrentUser = profile != null ? new User { Id = profile.Id, Name = profile.Name, Role = profile.Role } : null,
                Profile = profile
            };

            return View("~/Views/Shared/Profile.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfile updated, string currentPassword, string newPassword, string confirmPassword, IFormFile photoFile)
        {
            System.Diagnostics.Debug.WriteLine("=== UPDATE PROFILE POST CALLED SUCCESS ===");
            if (updated == null || string.IsNullOrEmpty(updated.Id))
            {
                return RedirectToAction("Profile", new { userId = updated?.Id ?? "ADM-01" });
            }

            var existing = _context.UserProfiles.FirstOrDefault(p => p.Id == updated.Id);
            if (existing != null)
            {
                existing.Name = updated.Name;
                existing.Email = updated.Email;
                existing.Phone = updated.Phone;
                existing.Address = updated.Address;
                existing.DateOfBirth = updated.DateOfBirth;
                existing.Gender = updated.Gender;
                existing.Username = updated.Username;

                // Hubinta Furaha Sirta ah (Password Change)
                if (!string.IsNullOrEmpty(newPassword))
                {
                    if (currentPassword == existing.Password && newPassword == confirmPassword)
                    {
                        existing.Password = newPassword;
                    }
                }

                _context.Entry(existing).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.SaveChanges();
            }

            return RedirectToAction("Profile", new { userId = updated.Id });
        }
    }
}