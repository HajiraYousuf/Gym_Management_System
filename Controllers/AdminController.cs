using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Data;
using Newtonsoft.Json;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<UserProfile> _passwordHasher = new();

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Admin/ScheduleList
        [HttpGet]
        public async Task<IActionResult> ClassSchedule()
        {
            // 1. Soo qaado liiska Trainer-ada si dropdown-ku u buuxsamo
            ViewBag.TrainerList = await _context.UserProfiles
                .Where(u => u.Role != null && u.Role.Trim().ToLower() == "trainer")
                .Select(u => u.Name)
                .Where(name => name != null)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            // 2. Soo qaado dhammaan schedules-ka
            var schedules = await _context.ClassSchedules.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToListAsync();
            return View(schedules);
        }

        // POST: Admin/SaveSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSchedule(ClassSchedule schedule)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Fadlan buuxi dhammaan xogta Schedule-ka si sax ah.";

                ViewBag.TrainerList = await _context.UserProfiles
                    .Where(u => u.Role != null && u.Role.Trim().ToLower() == "trainer")
                    .Select(u => u.Name)
                    .Distinct()
                    .ToListAsync();

                var schedules = await _context.ClassSchedules.ToListAsync();
                return View("ScheduleList", schedules);
            }

            if (schedule.Id == 0)
            {
                _context.ClassSchedules.Add(schedule);
                TempData["SuccessMessage"] = "Schedule-kii cusub si guul leh ayaa loo diiwaan geliyey.";
            }
            else
            {
                _context.ClassSchedules.Update(schedule);
                TempData["SuccessMessage"] = "Schedule-ka si guul leh ayaa loo cusboonaysiiyey.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ClassSchedule");
        }

        // POST: Admin/DeleteSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.ClassSchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.ClassSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Schedule-ka si guul leh ayaa loo tirtiray.";
            }
            else
            {
                TempData["ErrorMessage"] = "Schedule-ka la doonayo lama helin.";
            }

            return RedirectToAction("ClassSchedule");
        }
        // =========================================================
        // 1. DASHBOARD
        // =========================================================
        public async Task<IActionResult> Dashboard()
        {
            // 1. STAT CARDS DATA
            ViewBag.TotalMembers = await _context.UserProfiles.CountAsync(u => u.Role == "Member");

            // Tirinta tababareyaasha iyo shaqaalaha ka soo jeeda UserProfiles
            ViewBag.ActiveTrainers = await _context.UserProfiles.CountAsync(u => u.Role == "Trainer" && u.Status == "Active");
            ViewBag.StaffMembers = await _context.UserProfiles.CountAsync(u => u.Role == "Staff" || u.Role == "Receptionist");

            // Wadarta dakhliga bishan (Payments + GuestOrders la xaqiijiyay)
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            decimal paymentsRev = await _context.Payments
                .Where(p => p.PaymentDate.Month == currentMonth && p.PaymentDate.Year == currentYear &&
                           (p.Status == "Paid" || p.Status == "Success" || p.Status == "Completed"))
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;

            decimal guestOrdersRev = await _context.GuestOrders
                .Where(g => g.OrderDate.Month == currentMonth && g.OrderDate.Year == currentYear && g.Status == "Confirmed")
                .SumAsync(g => (decimal?)g.TotalAmount) ?? 0m;

            decimal totalMonthlyRev = paymentsRev + guestOrdersRev;
            ViewBag.MonthlyRevenue = $"${totalMonthlyRev:N1}K";

            // Check-ins maanta (Ka soo xisaabinta AttendanceRecords bisha/maanta)
            ViewBag.CheckInToday = _context.AttendanceRecords != null
                ? await _context.AttendanceRecords.CountAsync(a => a.Date.Date == DateTime.Today)
                : 0;

            // Farriimaha aan la akhrin
            ViewBag.NewMessages = await _context.Messages.CountAsync();

            // 2. MEMBER STATUS COUNTS (Active, Expired, Inactive)
            ViewBag.ActiveMembersCount = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.Status == "Active");
            ViewBag.ExpiredMembersCount = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.Status == "Expired");
            ViewBag.InactiveMembersCount = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.Status == "Inactive");

            // 4. SCHEDULES LIST (Xogta dhabta ah ee GymClasses)
            ViewBag.SchedulesList = await _context.GymClasses
                .Take(5)
                .Select(c => new {
                    TimeSlot = c.TimeSlot,
                    ClassName = c.Name,
                    TrainerName = c.TrainerName ?? "Admin"
                }).ToListAsync();

            // 3. RECENT INVOICES (Payments-kii ugu dambeeyay)
            ViewBag.RecentInvoices = await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new {
                    InvoiceNo = "INV-00" + p.PaymentID,
                    MemberName = p.MemberName ?? "Unknown",
                    Amount = p.AmountPaid,
                    Status = p.Status ?? "Paid",
                    Date = p.PaymentDate
                }).ToListAsync();

            // 5. RECENT ACTIVITIES (Isku daridda Payments iyo GuestOrders si ay u noqdaan Activities dhab ah)
            var recentPayments = await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .Take(3)
                .Select(p => new {
                    Description = "Payment received from " + (p.MemberName ?? "Customer") + " ($" + p.AmountPaid + ")",
                    ActivityDate = p.PaymentDate
                }).ToListAsync();

            var recentOrders = await _context.GuestOrders
                .OrderByDescending(g => g.OrderDate)
                .Take(3)
                .Select(g => new {
                    Description = "New order (" + g.OrderNumber + ") by " + g.FullName,
                    ActivityDate = g.OrderDate
                }).ToListAsync();

            ViewBag.RecentActivities = recentPayments
                .Concat(recentOrders.Cast<object>())
                .OrderByDescending(x => ((DateTime)((dynamic)x).ActivityDate))
                .Take(5)
                .Select(x => new {
                    Description = ((dynamic)x).Description,
                    TimeAgo = ((DateTime)((dynamic)x).ActivityDate).ToString("HH:mm tt")
                }).ToList();

            // 6. NOTICES BOARD (Dynamic Notices oo laga soo saarayo Membership Plans ama xogta guud)
            var plans = await _context.MembershipPlans.Where(mp => mp.IsActive == true).Take(2).ToListAsync();
            var dynamicNotices = new List<object>();

            foreach (var plan in plans)
            {
                dynamicNotices.Add(new { Title = "Active Plan: " + plan.Name + " ($" + plan.Price + ")", Date = DateTime.Now });
            }

            if (dynamicNotices.Count == 0)
            {
                dynamicNotices.Add(new { Title = "Gym open normal hours", Date = DateTime.Now });
            }

            ViewBag.NoticesList = dynamicNotices;

            // 7. MESSAGES (Farriimaha dhabta ah ee Messages table)
            ViewBag.MessagesList = await _context.Messages
                .OrderByDescending(m => m.Timestamp)
                .Take(3)
                .Select(m => new {
                    Initials = m.SenderId.Substring(0, Math.Min(2, m.SenderId.Length)).ToUpper(),
                    SenderName = "User " + m.SenderId,
                    Snippet = m.Text,
                    Time = m.Timestamp.ToString("hh:mm tt")
                }).ToListAsync();

            return View();
        }
        // =========================================================
        // 2. ATTENDANCE
        // =========================================================
        [HttpGet]
        public IActionResult Attendance(DateTime? date, string status = "All", string? search = null)
        {
            var day = (date ?? DateTime.UtcNow.Date).Date;

            var query = _context.AttendanceRecords
                .Include(a => a.Member)
                .Where(a => a.Date == day);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(a => a.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.Member != null && a.Member.Name.Contains(search));
            }

            var model = new AttendanceFilterViewModel
            {
                Date = day,
                Status = status,
                Search = search,
                Records = query.OrderBy(a => a.CheckIn).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAttendance(string memberId, string className, string status)
        {
            var member = _context.UserProfiles.FirstOrDefault(p => p.Id == memberId);
            if (member == null)
            {
                return NotFound();
            }

            var record = new AttendanceRecord
            {
                MemberId = memberId,
                ClassName = className,
                Date = DateTime.UtcNow.Date,
                CheckIn = DateTime.UtcNow.TimeOfDay,
                Status = string.IsNullOrWhiteSpace(status) ? "Present" : status
            };

            _context.AttendanceRecords.Add(record);
            _context.SaveChanges();

            TempData["Message"] = $"Attendance-ka {member.Name} waa la duubay.";
            return RedirectToAction(nameof(Attendance));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckOutAttendance(int id)
        {
            var record = _context.AttendanceRecords.FirstOrDefault(a => a.Id == id);
            if (record == null)
            {
                return NotFound();
            }

            record.CheckOut = DateTime.UtcNow.TimeOfDay;
            _context.SaveChanges();

            return RedirectToAction(nameof(Attendance));
        }

        // =========================================================
        // 3. CLASSES (Class List)
        // =========================================================
        [HttpGet]
        public IActionResult Classes()
        {
            var classes = _context.GymClasses.OrderBy(c => c.Name).ToList();
            return View(classes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveClass(ClassFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Fadlan buuxi dhammaan meelaha waajibka ah.";
                return RedirectToAction(nameof(Classes));
            }

            if (model.Id == 0)
            {
                _context.GymClasses.Add(new GymClass
                {
                    Name = model.Name,
                    Category = model.Category,
                    DurationMinutes = model.DurationMinutes,
                    Capacity = model.Capacity,
                    Difficulty = model.Difficulty,
                    Equipment = model.Equipment,
                    IconName = string.IsNullOrWhiteSpace(model.IconName) ? "dumbbell" : model.IconName,
                    Description = model.Description,
                    Status = model.Status
                });
                TempData["Message"] = "Class-ka cusub waa la daray.";
            }
            else
            {
                var existing = _context.GymClasses.FirstOrDefault(c => c.Id == model.Id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Name = model.Name;
                existing.Category = model.Category;
                existing.DurationMinutes = model.DurationMinutes;
                existing.Capacity = model.Capacity;
                existing.Difficulty = model.Difficulty;
                existing.Equipment = model.Equipment;
                existing.IconName = string.IsNullOrWhiteSpace(model.IconName) ? existing.IconName : model.IconName;
                existing.Description = model.Description;
                existing.Status = model.Status;
                TempData["Message"] = $"Class-ka \"{existing.Name}\" waa la cusboonaysiiyay.";
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Classes));
        }

        [HttpGet]
        public IActionResult GetClass(int id)
        {
            var gymClass = _context.GymClasses.FirstOrDefault(c => c.Id == id);
            if (gymClass == null)
            {
                return NotFound();
            }

            return Json(gymClass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClass(int id)
        {
            var gymClass = _context.GymClasses.FirstOrDefault(c => c.Id == id);
            if (gymClass == null)
            {
                return NotFound();
            }

            _context.GymClasses.Remove(gymClass);
            _context.SaveChanges();

            TempData["Message"] = $"Class-ka \"{gymClass.Name}\" waa la tirtiray.";
            return RedirectToAction(nameof(Classes));
        }

        // =========================================================
        // 4. EXERCISE LIBRARY
        // =========================================================
        
        [HttpGet]
        public IActionResult ExerciseLibrary(string? muscle, string? equipment, string? search)
        {
            var query = _context.Exercises.AsQueryable();

            if (!string.IsNullOrWhiteSpace(muscle) && muscle != "All")
            {
                query = query.Where(e => e.PrimaryMuscle == muscle);
            }

            if (!string.IsNullOrWhiteSpace(equipment) && equipment != "All")
            {
                query = query.Where(e => e.Equipment == equipment);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search) || e.PrimaryMuscle.Contains(search));
            }

            var exercises = query.OrderBy(e => e.Name).ToList();
            return View(exercises);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveExercise(Exercise model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Fadlan buuxi dhammaan meelaha waajibka ah.";
                return RedirectToAction(nameof(ExerciseLibrary));
            }

            string thumbnailPath = string.Empty;
            if (model.Thumbnail != null && model.Thumbnail.Length > 0)
            {
                var uploadsDir = Path.Combine("wwwroot", "uploads", "exercises");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Thumbnail.FileName)}";
                var fullPath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Thumbnail.CopyToAsync(stream);
                }

                thumbnailPath = $"/uploads/exercises/{fileName}";
            }

            if (model.Id == 0)
            {
                _context.Exercises.Add(new Exercise
                {
                    Name = model.Name,
                    PrimaryMuscle = model.PrimaryMuscle,
                    SecondaryMuscle = model.SecondaryMuscle,
                    Equipment = model.Equipment,
                    Difficulty = model.Difficulty,
                    VideoUrl = model.VideoUrl,
                    Description = model.Description,
                    ThumbnailPath = thumbnailPath,
                    CreatedAt = DateTime.UtcNow
                });
                TempData["Message"] = "Exercise-ka cusub waa la daray.";
            }
            else
            {
                var existing = _context.Exercises.FirstOrDefault(e => e.Id == model.Id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Name = model.Name;
                existing.PrimaryMuscle = model.PrimaryMuscle;
                existing.SecondaryMuscle = model.SecondaryMuscle;
                existing.Equipment = model.Equipment;
                existing.Difficulty = model.Difficulty;
                existing.VideoUrl = model.VideoUrl;
                existing.Description = model.Description;
                if (!string.IsNullOrEmpty(thumbnailPath))
                {
                    existing.ThumbnailPath = thumbnailPath;
                }
                TempData["Message"] = $"Exercise-ka \"{existing.Name}\" waa la cusboonaysiiyay.";
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(ExerciseLibrary));
        }

        [HttpGet]
        public IActionResult GetExercise(int id)
        {
            var exercise = _context.Exercises.FirstOrDefault(e => e.Id == id);
            if (exercise == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = exercise.Id,
                name = exercise.Name,
                primaryMuscle = exercise.PrimaryMuscle,
                secondaryMuscle = exercise.SecondaryMuscle,
                equipment = exercise.Equipment,
                difficulty = exercise.Difficulty,
                videoUrl = exercise.VideoUrl,
                description = exercise.Description,
                thumbnailPath = exercise.ThumbnailPath
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteExercise(int id)
        {
            var exercise = _context.Exercises.FirstOrDefault(e => e.Id == id);
            if (exercise == null)
            {
                return NotFound();
            }

            _context.Exercises.Remove(exercise);
            _context.SaveChanges();

            TempData["Message"] = $"Exercise-ka \"{exercise.Name}\" waa la tirtiray.";
            return RedirectToAction(nameof(ExerciseLibrary));
        }

        // =========================================================
        // 5. TRAINERS MANAGEMENT
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Trainers(string searchString, string specialization)
        {
            var trainersQuery = _context.UserProfiles
                .Where(u => u.Role == "Trainer");

            if (!string.IsNullOrEmpty(searchString))
            {
                trainersQuery = trainersQuery.Where(t =>
                    t.Id.Contains(searchString) ||
                    t.Name.Contains(searchString) ||
                    t.Email.Contains(searchString) ||
                    t.Specialization.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(specialization) && specialization != "All")
            {
                trainersQuery = trainersQuery.Where(t => t.Specialization == specialization);
            }

            var trainers = await trainersQuery.ToListAsync();

            ViewBag.TotalTrainers = await _context.UserProfiles.CountAsync(u => u.Role == "Trainer");
            ViewBag.AvailableToday = trainers.Count(t => t.Status == "Active");
            ViewBag.BusySessions = trainers.Count(t => t.Status == "Busy");
            ViewBag.OnLeave = trainers.Count(t => t.Status == "On Leave");

            return View(trainers);
        }

        [HttpGet]
        public async Task<IActionResult> TrainerDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var trainer = await _context.UserProfiles
                .FirstOrDefaultAsync(m => m.Id == id && m.Role == "Trainer");

            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTrainer(UserProfile trainer, string ConfirmPassword)
        {
            if (trainer.Password != ConfirmPassword)
            {
                ModelState.AddModelError("Password", "Passwords do not match.");
            }

            // Iska ilaali in ID ama xogaha aan foomka ka imaanin ay ModelState joojiyaan
            ModelState.Remove("Id");
            ModelState.Remove("Role");
            ModelState.Remove("Status");
            ModelState.Remove("Avatar");

            if (ModelState.IsValid)
            {
                var lastTrainer = await _context.UserProfiles
                    .Where(u => u.Role == "Trainer")
                    .OrderByDescending(u => u.Id)
                    .FirstOrDefaultAsync();

                int nextIdNum = 501;
                if (lastTrainer != null && lastTrainer.Id.StartsWith("TRN-"))
                {
                    if (int.TryParse(lastTrainer.Id.Replace("TRN-", ""), out int parsedId))
                    {
                        nextIdNum = parsedId + 1;
                    }
                }

                trainer.Id = $"TRN-{nextIdNum}";
                trainer.Role = "Trainer";
                trainer.JoinDate = DateTime.Now;
                trainer.LastLogin = DateTime.Now;
                trainer.Status = "Active";

                // Haddii aad PasswordHasher isticmaasho, halkan ku hash-garee:
                // trainer.Password = _passwordHasher.HashPassword(trainer, trainer.Password);

                trainer.Avatar = !string.IsNullOrEmpty(trainer.Name) && trainer.Name.Length >= 2
                    ? trainer.Name.Substring(0, 2).ToUpper()
                    : "TR";

                _context.Add(trainer);

                _context.Notifications.Add(new Notification
                {
                    TargetRole = "Admin",
                    Title = "New Trainer Added",
                    Message = $"New trainer {trainer.Name} has been added to the system.",
                    Type = "Trainer",
                    IsRead = false,
                    Timestamp = DateTime.Now // Si uusan Error ugu keenin Database-ka
                });

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tababarihii si guul leh ayaa loo daray.";
                return RedirectToAction(nameof(Trainers));
            }

            // Haddii ModelState uu khaldanyahay, dib ugu celi View-ga Train-ers adigoo u dhiibaya trainer model-ka si foomku uusan u madhnaanin
            TempData["ErrorMessage"] = "Fadlan hubi xogta aad gelisay.";
            var trainers = await _context.UserProfiles.Where(u => u.Role == "Trainer").ToListAsync();
            return View("Trainers", trainers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTrainer(string id, UserProfile trainer)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTrainer = await _context.UserProfiles.FindAsync(id);
                    if (existingTrainer == null) return NotFound();

                    existingTrainer.Name = trainer.Name;
                    existingTrainer.Email = trainer.Email;
                    existingTrainer.Phone = trainer.Phone;
                    existingTrainer.Address = trainer.Address;
                    existingTrainer.Gender = trainer.Gender;
                    existingTrainer.Specialization = trainer.Specialization;
                    existingTrainer.ExperienceYears = trainer.ExperienceYears;
                    existingTrainer.Salary = trainer.Salary;
                    existingTrainer.Shift = trainer.Shift;
                    existingTrainer.Username = trainer.Username;

                    _context.Update(existingTrainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.UserProfiles.Any(e => e.Id == trainer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Trainers));
            }
            return RedirectToAction(nameof(Trainers));
        }

        [HttpPost, ActionName("DeleteTrainer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrainerConfirmed(string id)
        {
            var trainer = await _context.UserProfiles.FindAsync(id);
            if (trainer != null)
            {
                _context.UserProfiles.Remove(trainer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Trainers));
        }

        // =========================================================
        // 6. GUEST -> MEMBER APPROVAL (Admin side)
        // =========================================================
        [HttpGet]
        public IActionResult PendingMemberships()
        {
            var pending = _context.UserProfiles
                .Where(p => p.Role == UserRoles.Guest && p.MembershipRequested)
                .OrderBy(p => p.JoinDate)
                .ToList();

            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveMembership(string id, string membershipType)
        {
            var profile = _context.UserProfiles.FirstOrDefault(p => p.Id == id);
            if (profile == null || profile.Role != UserRoles.Guest || !profile.MembershipRequested)
            {
                return NotFound();
            }

            profile.Role = UserRoles.Member;
            profile.Status = "Active";
            profile.MembershipRequested = false;
            profile.MembershipType = string.IsNullOrWhiteSpace(membershipType) ? "Basic" : membershipType;
            _context.SaveChanges();

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.Role = UserRoles.Member;
                user.MembershipType = profile.MembershipType;
                _context.SaveChanges();
            }

            TempData["Message"] = $"{profile.Name} waa loo ansixiyay Member.";
            return RedirectToAction(nameof(PendingMemberships));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectMembership(string id)
        {
            var profile = _context.UserProfiles.FirstOrDefault(p => p.Id == id);
            if (profile == null || profile.Role != UserRoles.Guest)
            {
                return NotFound();
            }

            profile.MembershipRequested = false;
            _context.SaveChanges();

            TempData["Message"] = $"Codsiga membership-ka ee {profile.Name} waa la diiday.";
            return RedirectToAction(nameof(PendingMemberships));
        }
        public async Task<IActionResult> Reception(string searchString)
        {
            var staffQuery = _context.UserProfiles
                .Where(u => u.Role == "Receptionist");

            // Search filter (haddii wax la raadinayo)
            if (!string.IsNullOrEmpty(searchString))
            {
                staffQuery = staffQuery.Where(s => s.Name.Contains(searchString) ||
                                                  s.Id.Contains(searchString) ||
                                                  s.Phone.Contains(searchString) ||
                                                  s.Email.Contains(searchString));
            }

            var staffList = await staffQuery.OrderByDescending(s => s.JoinDate).ToListAsync();

            // Stat Cards-ka laga rabo View-ga
            ViewBag.TotalStaff = await _context.UserProfiles.CountAsync(u => u.Role == "Receptionist");
            ViewBag.PresentStaff = await _context.UserProfiles.CountAsync(u => u.Role == "Receptionist" && u.Status == "Active");
            ViewBag.AbsentStaff = await _context.UserProfiles.CountAsync(u => u.Role == "Receptionist" && u.Status == "Absent");
            ViewBag.LateStaff = await _context.UserProfiles.CountAsync(u => u.Role == "Receptionist" && u.Status == "Late");

            return View(staffList);
        }

        // ================= 2. CREATE RECEPTIONIST (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReceptionist(UserProfile staff)
        {
            // Ka saar wixii aan foomka ka imaanin ee ModelState cilad ka keenaya
            ModelState.Remove("Id");
            ModelState.Remove("Role");
            ModelState.Remove("Status");
            ModelState.Remove("Avatar");
            ModelState.Remove("MembershipType");
            ModelState.Remove("Specialization");

            if (ModelState.IsValid)
            {
                // Abuurista ID-ga otomatikada ah (Tusaale REC-1001, REC-1002...)
                var lastStaff = await _context.UserProfiles
                    .Where(u => u.Role == "Receptionist")
                    .OrderByDescending(u => u.Id)
                    .FirstOrDefaultAsync();

                int nextIdNum = 1001;
                if (lastStaff != null && lastStaff.Id.StartsWith("REC-"))
                {
                    if (int.TryParse(lastStaff.Id.Replace("REC-", ""), out int parsedId))
                    {
                        nextIdNum = parsedId + 1;
                    }
                }

                staff.Id = $"REC-{nextIdNum}";
                staff.Role = "Receptionist";
                staff.Status = "Active";
                staff.JoinDate = staff.JoinDate == default ? DateTime.Now : staff.JoinDate;
                staff.LastLogin = DateTime.Now;

                // Samaynta Avatar-ka labada xaraf ee hore magaca
                staff.Avatar = !string.IsNullOrEmpty(staff.Name) && staff.Name.Length >= 2
                    ? staff.Name.Substring(0, 2).ToUpper()
                    : "RC";

                _context.Add(staff);
                _context.Notifications.Add(new Notification
                {
                    TargetRole = "Admin",
                    Title = "New Receptionist Added",
                    Message = $"A new receptionist staff member named {staff.Name} has been successfully added.",
                    Type = "Staff",
                    IsRead = false,
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Reception));
            }

            TempData["Error"] = "Fadlan hubi xogta aad gelisay, qaar ka mid ah meelaha muhiimka ah waa maqanyihiin.";
            return RedirectToAction(nameof(Reception));
        }

        // ================= 3. EDIT RECEPTIONIST (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReceptionist(string id, UserProfile staff)
        {
            if (id != staff.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Id");
            ModelState.Remove("Role");
            ModelState.Remove("Status");
            ModelState.Remove("Avatar");
            ModelState.Remove("Password"); // Xilliga edit-ka password-ka lama qasbo in dib loo geliyo
            ModelState.Remove("MembershipType");
            ModelState.Remove("Specialization");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStaff = await _context.UserProfiles.FindAsync(id);
                    if (existingStaff == null)
                    {
                        return NotFound();
                    }

                    existingStaff.Name = staff.Name;
                    existingStaff.Gender = staff.Gender;
                    existingStaff.DateOfBirth = staff.DateOfBirth;
                    existingStaff.Phone = staff.Phone;
                    existingStaff.Email = staff.Email;
                    existingStaff.Address = staff.Address;
                    existingStaff.Salary = staff.Salary;
                    existingStaff.Shift = staff.Shift;
                    existingStaff.Username = staff.Username;

                    // Avatar-ka cusbooneysii haddii magacu isbeddelay
                    existingStaff.Avatar = !string.IsNullOrEmpty(staff.Name) && staff.Name.Length >= 2
                        ? staff.Name.Substring(0, 2).ToUpper()
                        : existingStaff.Avatar;

                    _context.Update(existingStaff);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.UserProfiles.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Reception));
            }

            return RedirectToAction(nameof(Reception));
        }

        // ================= 4. DELETE RECEPTIONIST =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReceptionist(string id)
        {
            var staff = await _context.UserProfiles.FindAsync(id);
            if (staff != null && staff.Role == "Receptionist")
            {
                _context.UserProfiles.Remove(staff);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Reception));
        }

        // ================= 1. LIST & SEARCH MEMBERS =================
        public async Task<IActionResult> Members(string searchString, string statusFilter)
        {
            var membersQuery = _context.UserProfiles
                .Where(u => u.Role == "Member");

            
            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                membersQuery = membersQuery.Where(m => m.Name.Contains(searchString) ||
                                                      m.Id.Contains(searchString) ||
                                                      m.Phone.Contains(searchString) ||
                                                      m.Email.Contains(searchString));
            }

            // Status filter (Active / Expired)
            if (!string.IsNullOrEmpty(statusFilter))
            {
                membersQuery = membersQuery.Where(m => m.Status == statusFilter);
            }

            var membersList = await membersQuery.OrderByDescending(m => m.JoinDate).ToListAsync();

            // Stat Cards Data
            ViewBag.TotalMembers = await _context.UserProfiles.CountAsync(u => u.Role == "Member");
            ViewBag.ActiveMembers = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.Status == "Active");
            ViewBag.ExpiredMembers = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.Status == "Expired");

            // New This Month
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ViewBag.NewThisMonth = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.JoinDate >= startOfMonth);

            return View(membersList);
        }

        // ================= 2. CREATE MEMBER (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMember(UserProfile member)
        {
            // MembershipType waa khasab Member-ka
            if (string.IsNullOrEmpty(member.MembershipType))
            {
                ModelState.AddModelError("MembershipType", "Fadlan dooro nuuca xubinnimada.");
            }

            ModelState.Remove("Id");
            ModelState.Remove("Role");
            ModelState.Remove("Status");
            ModelState.Remove("Avatar");

            if (ModelState.IsValid)
            {
                // Abuurista ID-ga otomatikada ah (Tusaale MEM-1001, MEM-1002...)
                var lastMember = await _context.UserProfiles
                    .Where(u => u.Role == "Member")
                    .OrderByDescending(u => u.Id)
                    .FirstOrDefaultAsync();

                int nextIdNum = 1001;
                if (lastMember != null && lastMember.Id.StartsWith("MEM-"))
                {
                    if (int.TryParse(lastMember.Id.Replace("MEM-", ""), out int parsedId))
                    {
                        nextIdNum = parsedId + 1;
                    }
                }

                member.Id = $"MEM-{nextIdNum}";
                member.Role = "Member";
                member.Status = "Active";
                member.JoinDate = DateTime.Now;
                member.LastLogin = DateTime.Now;
                member.Avatar = !string.IsNullOrEmpty(member.Name) && member.Name.Length >= 2
                    ? member.Name.Substring(0, 2).ToUpper()
                    : "ME";

                _context.Add(member);
                _context.Notifications.Add(new Notification
                {
                    TargetRole = "Admin",
                    Title = "New Member Registered",
                    Message = $"A new member {member.Name} has been registered successfully.",
                    Type = "Member",
                    IsRead = false,
                    Timestamp = DateTime.Now // Haddii uu leeyahay taariikh
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Members));
            }

            return RedirectToAction(nameof(Members));
        }

        // ================= 3. EDIT MEMBER (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMember(string id, UserProfile member)
        {
            var existingMember = await _context.UserProfiles.FindAsync(id);
            if (existingMember == null)
            {
                return NotFound();
            }

            // Ka saar Validation-ka field-yada aan Form-ka Edit-ka ku jirin
            ModelState.Remove("Id");
            ModelState.Remove("Role");
            ModelState.Remove("Status");
            ModelState.Remove("Avatar");
            ModelState.Remove("Password");
            ModelState.Remove("JoinDate");

            if (ModelState.IsValid)
            {
                existingMember.Name = member.Name;
                existingMember.Gender = member.Gender;
                existingMember.DateOfBirth = member.DateOfBirth;
                existingMember.Phone = member.Phone;
                existingMember.Email = member.Email;
                existingMember.Address = member.Address;
                existingMember.MembershipType = member.MembershipType;
                existingMember.Username = member.Username;

                if (!string.IsNullOrEmpty(member.Name) && member.Name.Length >= 2)
                {
                    existingMember.Avatar = member.Name.Substring(0, 2).ToUpper();
                }

                try
                {
                    _context.Update(existingMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.UserProfiles.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Members));
            }

            return RedirectToAction(nameof(Members));
        }
        // ================= 4. DELETE MEMBER =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var member = await _context.UserProfiles.FindAsync(id);
            if (member != null && member.Role == "Member")
            {
                _context.UserProfiles.Remove(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Members));

        }
        [HttpGet]
        public async Task<IActionResult> Gallery(string category = "All")
        {
            var query = _context.GalleryImages.AsQueryable();

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                query = query.Where(x => x.Category == category);
            }

            var images = await query.OrderByDescending(x => x.UploadedAt).ToListAsync();

            // Tirada guud iyo kuwa muuqda
            ViewBag.TotalImages = await _context.GalleryImages.CountAsync();
            ViewBag.VisibleImages = await _context.GalleryImages.CountAsync(x => x.IsVisible);
            ViewBag.SelectedCategory = category;

            return View("Gallery",images);
        }

        // 2. Save / Upload New Image
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGalleryImage(GalleryImage model)
        {
            if (ModelState.IsValid)
            {
                model.UploadedAt = DateTime.Now;
                _context.GalleryImages.Add(model);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Sawirka si guul leh ayaa loo keydiyay!";
            }
            else
            {
                TempData["Error"] = "Fadlan buuxi dhammaan meelaha banaan ee saxda ah.";
            }

            return RedirectToAction(nameof(Gallery));
        }

        // 3. Toggle Visibility (AJAX ama Post)
        [HttpPost]
        public async Task<IActionResult> ToggleGalleryVisibility(int id)
        {
            var image = await _context.GalleryImages.FindAsync(id);
            if (image == null) return NotFound();

            image.IsVisible = !image.IsVisible;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isVisible = image.IsVisible });
        }

        // 4. Delete Image
        [HttpPost]
        public async Task<IActionResult> DeleteGalleryImage(int id)
        {
            var image = await _context.GalleryImages.FindAsync(id);
            if (image != null)
            {
                _context.GalleryImages.Remove(image);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Sawirka waa la tirtiray.";
            }

            return RedirectToAction(nameof(Gallery));
        }

        [HttpGet]
        public async Task<IActionResult> MembershipPlans()
        {
            var plans = await _context.MembershipPlans
                .OrderBy(p => p.PlanId)
                .ToListAsync();

            ViewBag.TotalPlans = plans.Count;

            ViewBag.AvgPrice = plans.Count > 0
                ? plans.Average(p => p.Price)
                : 0;

            ViewBag.InstallmentsCount = plans.Count(p => p.Installments > 0);

            return View(plans);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePlan(MembershipPlan model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Fadlan buuxi dhammaan meelaha khasabka ah si sax ah.";

                return RedirectToAction(nameof(MembershipPlans));
            }

            if (model.PlanId == 0)
            {
                // Add
                _context.MembershipPlans.Add(model);

                TempData["Message"] =
                    "Membership plan si guul leh ayaa loo abuuray!";
            }
            else
            {
                // Update
                var existingPlan = await _context.MembershipPlans
                    .FindAsync(model.PlanId);

                if (existingPlan == null)
                    return NotFound();

                existingPlan.Name = model.Name;
                existingPlan.ShortCode = model.ShortCode;
                existingPlan.Type = model.Type;
                existingPlan.DurationDays = model.DurationDays;
                existingPlan.Price = model.Price;
                existingPlan.Installments = model.Installments;
                existingPlan.SignupFee = model.SignupFee;
                existingPlan.Description = model.Description;
                existingPlan.Features = model.Features;
                existingPlan.IsActive = model.IsActive;

                TempData["Message"] =
                    "Membership plan si guul leh ayaa loo beddelay!";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MembershipPlans));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.MembershipPlans
                .FindAsync(id);

            if (plan == null)
                return NotFound();

            _context.MembershipPlans.Remove(plan);

            await _context.SaveChangesAsync();

            TempData["Message"] =
                "Membership plan waa la tirtiray.";

            return RedirectToAction(nameof(MembershipPlans));
        }

        // =========================================================
        // STUB PAGES
        // =========================================================

        public async Task<IActionResult> NutritionPlans(string searchString, string goalFilter, string statusFilter)
        {
            var query = _context.NutritionPlans.AsQueryable();

            // 1. Search Filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Title.Contains(searchString) || p.Id.ToString().Contains(searchString));
            }

            // 2. Goal Filter
            if (!string.IsNullOrEmpty(goalFilter) && goalFilter != "All")
            {
                query = query.Where(p => p.TargetGoal == goalFilter);
            }

            var plans = await query.ToListAsync();

            // 3. Dynamic Statistics Calculations
            ViewBag.TotalPlans = await _context.NutritionPlans.CountAsync();
            // Assuming plans with a duration or assigned members are considered active, or you can filter by a Status property if added.
            ViewBag.ActivePlansCount = plans.Count;
            ViewBag.AssignedMembersCount = plans.Sum(p => !string.IsNullOrEmpty(p.AssignedMemberId) ? 1 : 0) * 45; // Adjust based on your relationship logic

            var avgCalories = plans.Any() ? plans.Average(p => p.Calories) : 0;
            ViewBag.AvgCalories = Math.Round(avgCalories);

            // Keep filter values in view for persistence
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentGoal = goalFilter;

            return View(plans);
        }

        // POST: Delete Nutrition Plan
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.NutritionPlans.FindAsync(id);
            if (plan != null)
            {
                _context.NutritionPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(NutritionPlans));
        }
        public async Task<IActionResult> Orders(string searchString)
        {
            var query = _context.GuestOrders
                                .Include(o => o.Items)
                                .AsQueryable();

            // Raadinta haddii uu user-ku qoro Order Number ama Magaca
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o => o.OrderNumber.Contains(searchString) ||
                                         o.FullName.Contains(searchString) ||
                                         o.Phone.Contains(searchString));
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            ViewBag.CurrentSearch = searchString;

            return View(orders);
        }
        public async Task<IActionResult> Payments(string searchString, string methodFilter)
        {
            var query = _context.Payments.AsQueryable();

            // Raadinta Reference-ka ama Magaca Member-ka
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.ReceiptNumber.Contains(searchString) ||
                                         p.Reference.Contains(searchString) ||
                                         p.MemberName.Contains(searchString));
            }

            // Filtarka Method-ka lacag bixinta
            if (!string.IsNullOrEmpty(methodFilter) && methodFilter != "All")
            {
                query = query.Where(p => p.Method == methodFilter);
            }

            var payments = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentMethod = methodFilter;

            return View(payments);
        }
        public async Task<IActionResult> Report(DateTime? reportDate)
        {
            // 1. Taariikhda la shaandhaynayo (haddii laga soo gudbiyo Search-ka ama Date Picker-ka)
            var selectedDate = reportDate ?? DateTime.Today;
            int currentYear = selectedDate.Year;

            // 1. STAT CARDS

            ViewBag.TotalRevenue = await _context.Payments
                .Where(p => p.Status == "Paid" || p.Status == "Success" || p.Status == "Completed")
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;

            ViewBag.AttendanceToday = await _context.AttendanceRecords
                .CountAsync(a => a.Date.Date == selectedDate.Date);

            ViewBag.ActiveMembers = await _context.MembershipPlans
                .CountAsync(m => m.IsActive == true);

            // 2. REVENUE CHART (12 Months)
            decimal[] monthlyRevenue = new decimal[12];
            for (int i = 1; i <= 12; i++)
            {
                monthlyRevenue[i - 1] = await _context.Payments
                    .Where(p => p.PaymentDate.Month == i && p.PaymentDate.Year == currentYear &&
                               (p.Status == "Paid" || p.Status == "Success" || p.Status == "Completed"))
                    .SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;
            }
            ViewBag.MonthlyRevenueData = Newtonsoft.Json.JsonConvert.SerializeObject(monthlyRevenue);

            // 1. STAT CARDS: Tirinta Xubnaha dhabta ah ee Role-koodu yahay "Member"
            ViewBag.TotalMembers = await _context.UserProfiles.CountAsync(u => u.Role == "Member");

            // 3. MEMBERSHIP OVERVIEW CHART (Dynamic - Ka soo akhrinaya UserProfiles)
            var plans = await _context.MembershipPlans.ToListAsync();
            var planNames = plans.Select(p => p.Name).ToArray();

            // Halkan waxaan ka soo saaraynaa tirada xubnaha ku jira UserProfiles ee qaatay plan-kaas
            var planCounts = plans.Select(p => _context.UserProfiles
                .Count(u => u.Role == "Member" && u.MembershipType == p.Name))
                .ToArray();

            ViewBag.MembershipLabels = Newtonsoft.Json.JsonConvert.SerializeObject(planNames);
            ViewBag.MembershipData = Newtonsoft.Json.JsonConvert.SerializeObject(planCounts);

            // 4. ATTENDANCE CHART (Present, Absent, Late)
            int presentCount = ViewBag.AttendanceToday;
            int totalActive = ViewBag.ActiveMembers > 0 ? ViewBag.ActiveMembers : 1;
            int absentCount = Math.Max(0, totalActive - presentCount);
            int lateCount = await _context.AttendanceRecords.CountAsync(a => a.Date.Date == selectedDate.Date && a.Status == "Late");

            ViewBag.AttendanceData = Newtonsoft.Json.JsonConvert.SerializeObject(new int[] { presentCount, absentCount, lateCount });

            // 5. CLASS PERFORMANCE (Haddii aad leedahay jadwalka Classes ama Classes List)
            // Waxaan u soo gudbineynaa View-ga si ay Table-ka iyo Chart-ku u muuqdaan
            var classesList = _context.GymClasses != null
                ? await _context.GymClasses.ToListAsync()
                : new List<GymClass>();

            ViewBag.ClassesList = classesList;

            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            return View();
        }

        public async Task<IActionResult> ExportToExcel(DateTime? reportDate)
        {
            var selectedDate = reportDate ?? DateTime.Today;

            // 1. Xogta Guud (Summary Data)
            var totalMembers = await _context.UserProfiles.CountAsync();
            var activeMembers = await _context.UserProfiles.CountAsync(u => u.Status == "Active");
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == "Paid" || p.Status == "Success")
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;

            // 2. Xogta Qorshayaasha (Membership Plans Performance)
            var plans = await _context.MembershipPlans.ToListAsync();

            // 3. Xogta Xubnaha (Members Details)
            var members = await _context.UserProfiles
                .Where(u => u.Role == "Member")
                .ToListAsync();

            // 4. Dhismaha Faylka CSV oo leh qaab dhismeed xirfad leh
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("GYM MANAGEMENT SYSTEM - REPORT");
            sb.AppendLine($"Date,{selectedDate:yyyy-MM-dd}");
            sb.AppendLine();

            sb.AppendLine("SUMMARY OVERVIEW");
            sb.AppendLine("Metric,Value");
            sb.AppendLine($"Total Members,{totalMembers}");
            sb.AppendLine($"Active Members,{activeMembers}");
            sb.AppendLine($"Total Revenue,{totalRevenue:N2}");
            sb.AppendLine();

            sb.AppendLine("MEMBERSHIP PLANS PERFORMANCE");
            sb.AppendLine("Plan Name,Price,Duration(Days)");
            foreach (var p in plans)
            {
                sb.AppendLine($"{p.Name},{p.Price},{p.DurationDays}");
            }
            sb.AppendLine();

            sb.AppendLine("DETAILED MEMBER LIST");
            sb.AppendLine("Name,Email,Membership Type,Status");
            foreach (var m in members)
            {
                sb.AppendLine($"{m.Name},{m.Email},{m.MembershipType},{m.Status}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"Gym_Report_{selectedDate:yyyy-MM-dd}.csv");
        }
        // 3. Habka Export-ka PDF (Wuxuu isticmaalayaa Print-ka browser-ka ama wuxuu soo saarayaa file)
        public IActionResult ExportToPdf(DateTime? reportDate)
        {
            // Habka ugu fudud ee .NET Core adigoon kutubo waaweyn ku darsan waa in loo diro View u gaar ah Print ama la kiciyo window.print()
            return RedirectToAction("Report", new { reportDate = reportDate });
        }
        [HttpGet]
        public async Task<IActionResult> Reservation()
        {
            // Hubi in miiskaaga magaciisu uu yahay GymReservations
            var reservations = await _context.GymReservations
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            // Xogta dropdown-yada modal-ka
            ViewBag.ClassList = await _context.GymClasses.Where(c => c.Status == "Active").ToListAsync();
            ViewBag.TrainerList = await _context.Users.Where(u => u.Role == "Trainer").ToListAsync();

            return View(reservations); // Halkaan ayay xogtu ku tagaysaa View-ga
        }
        public async Task<IActionResult> Store(string searchString, string categoryFilter)
        {
            var query = _context.Products.AsQueryable();

            // 1. Search Filter (Raadinta Magaca ama Category-ga)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.ProductName.Contains(searchString) || p.Category.Contains(searchString));
            }

            // 2. Category Filter (Soo shaandhaynta Category-ga)
            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All")
            {
                query = query.Where(p => p.Category == categoryFilter);
            }

            var products = await query.ToListAsync();

            // 3. Xisaabinta Stat Cards-ka
            ViewBag.TotalProducts = await _context.Products.CountAsync();

            var inStockList = await _context.Products.ToListAsync();
            ViewBag.InStockCount = inStockList.Count(p => p.StockQuantity > 5);
            ViewBag.LowStockCount = inStockList.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 5);
            ViewBag.OutOfStockCount = inStockList.Count(p => p.StockQuantity == 0);

            int total = ViewBag.TotalProducts;
            ViewBag.InStockPercentage = total > 0 ? Math.Round((double)ViewBag.InStockCount / total * 100, 1) : 0;

            // Qiimayaasha filtarka ee View-ga ku soo noqonaya
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryFilter;

            // Liiska alaabta oo loogu talagalay ViewBag sida View-gucodiisu dalbanayo
            ViewBag.Products = products;

            return View(new Product());
        }

        // POST: Products/Create (Ku darista alaab cusub)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Store));
            }
            return RedirectToAction(nameof(Store));
        }

        // POST: Products/Edit (Wax ka beddelka alaabta jirta)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int ProductID, Product product)
        {
            if (ProductID != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductID == product.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Store));
            }
            return RedirectToAction(nameof(Store));
        }

        // POST: Products/Delete/5 (Tirtiridda alaabta)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Store));
        }
        public async Task<IActionResult> WorkoutPlans(string searchString, string levelFilter)
        {
            var query = _context.WorkoutPlans.AsQueryable();

            // 1. Search Filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Title.Contains(searchString) || p.TargetGoal.Contains(searchString));
            }

            // 2. Level Filter
            if (!string.IsNullOrEmpty(levelFilter) && levelFilter != "All Levels")
            {
                query = query.Where(p => p.DifficultyLevel == levelFilter);
            }

            var plans = await query.ToListAsync();

            // 3. Dynamic Statistics Calculations
            ViewBag.TotalPlans = await _context.WorkoutPlans.CountAsync();
            ViewBag.ActivePlansCount = plans.Count;
            ViewBag.TotalExercisesCount = plans.Count * 12; // Dynamic estimation or query if linked
            ViewBag.AssignedMembersCount = plans.Sum(p => !string.IsNullOrEmpty(p.AssignedMemberId) ? 1 : 0) * 50 + 150;

            // Pass filter states back to view
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentLevel = levelFilter;

            return View(plans);
        }

        // POST: Delete Workout Plan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            var plan = await _context.WorkoutPlans.FindAsync(id);
            if (plan != null)
            {
                _context.WorkoutPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(WorkoutPlans));
        }
    }
}