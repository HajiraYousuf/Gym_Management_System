using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        public IActionResult Messages(string userId = "member1", string activeContactId = null)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? userId;

            var currentUser = _context.Users.FirstOrDefault(u => u.Id == currentUserId) ?? _context.Users.FirstOrDefault();
            var allowedContacts = new List<User>();

            if (currentUser != null)
            {
                // Ka saar shardi-ga chattedUserIds si dhamaan dadka la ogol yahay oo dhan ay u soo baxaan
                if (currentUser.Role == "Member")
                {
                    allowedContacts = _context.Users
                        .Where(u => u.Id != currentUser.Id && (u.Role == "Trainer" || u.Role == "Admin" || u.Role == "Receptionist"))
                        .ToList();
                }
                else if (currentUser.Role == "Trainer")
                {
                    allowedContacts = _context.Users
                        .Where(u => u.Id != currentUser.Id && (u.Role == "Admin" || u.Role == "Receptionist" || u.Role == "Member"))
                        .ToList();
                }
                else // Admin ama Receptionist
                {
                    allowedContacts = _context.Users
                        .Where(u => u.Id != currentUser.Id)
                        .ToList();
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
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(senderId) && !string.IsNullOrEmpty(receiverId))
            {
                var newMessage = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Text = text.Trim(),
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
        public IActionResult MarkAllAsRead(string role)
        {
            var unreadNotifs = _context.Notifications
                .Where(n => (n.TargetRole == role || n.TargetRole == "All") && !n.IsRead)
                .ToList();

            foreach (var notif in unreadNotifs)
            {
                notif.IsRead = true;
            }

            _context.SaveChanges();

            return RedirectToAction("Notifications", new { role = role });
        }

        [HttpPost]
        public IActionResult DismissNotification(int id, string role)
        {
            var notif = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (notif != null)
            {
                _context.Notifications.Remove(notif);
                _context.SaveChanges();
            }

            return RedirectToAction("Notifications", new { role = role });
        }

        public IActionResult Profile(string userId = null)
        {
            // 1. Ka soo qaad ID-ga User-ka hadda Logged-in-ka ah
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? HttpContext.Session.GetString("UserId")
                                   ?? userId;

            // 2. Ka raadi Profile-ka database-ka
            UserProfile profile = null;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                profile = _context.UserProfiles.FirstOrDefault(p => p.Id == currentUserId);
            }

            // 3. Haddii aan wali la helin, ka raadi userId-ga parameter-ka lagu soo dhiibay
            if (profile == null && !string.IsNullOrEmpty(userId))
            {
                profile = _context.UserProfiles.FirstOrDefault(p => p.Id == userId);
            }

            // 4. Haddii uu wali meelna ka weydo, ka soo qaad kan ugu horreeya
            if (profile == null)
            {
                profile = _context.UserProfiles.FirstOrDefault();
            }

            var viewModel = new SharedModel
            {
                CurrentUser = profile != null ? new User { Id = profile.Id, Name = profile.Name, Role = profile.Role } : null,
                Profile = profile
            };

            return View("~/Views/Shared/Profile.cshtml", viewModel);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfile updated, string currentPassword, string newPassword, string confirmPassword, IFormFile photoFile)
        {
            if (updated == null || string.IsNullOrEmpty(updated.Id))
            {
                return RedirectToAction("Profile");
            }

            var existing = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == updated.Id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "User profile ma la helin!";
                return RedirectToAction("Profile");
            }

            // 1. Cusbooneysii Xogta Caadiga ah
            existing.Name = updated.Name;
            existing.Email = updated.Email;
            existing.Phone = updated.Phone;
            existing.Address = updated.Address;
            existing.DateOfBirth = updated.DateOfBirth;
            existing.Gender = updated.Gender;
            existing.Username = updated.Username;

            // 2. Hubinta iyo Hashing-ka Password-ka Cusub
            if (!string.IsNullOrEmpty(newPassword))
            {
                var passwordHasher = new PasswordHasher<UserProfile>();

                // Hubi Current Password-ka hadda jira (Verify Hashed Password)
                var verificationResult = passwordHasher.VerifyHashedPassword(existing, existing.Password ?? string.Empty, currentPassword ?? string.Empty);

                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    TempData["ErrorMessage"] = "Password-kaaga hadda (Current Password) waa ma saxan!";
                    return RedirectToAction("Profile", new { userId = updated.Id });
                }

                // Hubi in New Password iyo Confirm Password ay isku mid yihiin
                if (newPassword != confirmPassword)
                {
                    TempData["ErrorMessage"] = "Password-ka cusub iyo Confirm Password-ku waa inay isku mid noqdaan!";
                    return RedirectToAction("Profile", new { userId = updated.Id });
                }

                // Hash-gareey password-ka cusub kahor intaan database-ka loo dirin
                existing.Password = passwordHasher.HashPassword(existing, newPassword);
            }

            _context.UserProfiles.Update(existing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile-kaaga si guul leh ayaa loo cusbooneysiiyay!";
            return RedirectToAction("Profile", new { userId = updated.Id });
        }
    }
}