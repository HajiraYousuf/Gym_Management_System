using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;
using static GymManagementSystem.Models.Payment;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Receptionist")]

    public class ReceptionistController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<UserProfile> _passwordHasher = new();

        public ReceptionistController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // 1. CheckInOut
        [HttpGet]
        public async Task<IActionResult> CheckInOut()
        {
            var today = DateTime.UtcNow.Date;

            // 1. Tirada Check-ins-ka maanta
            int todaysCheckIns = await _context.AttendanceRecords
                .CountAsync(a => a.Date == today && a.CheckIn != null);

            // 2. Dadka hadda ku jira gym-ka (CheckIn ayaa jira laakiin CheckOut ma jiro)
            var currentlyInsideList = await _context.AttendanceRecords
                .Include(a => a.Member)
                .Where(a => a.Date == today && a.CheckIn != null && a.CheckOut == null)
                .OrderByDescending(a => a.CheckIn)
                .ToListAsync();

            int currentlyInsideCount = currentlyInsideList.Count;

            // 3. Exits-kii ugu dambeeyay (Recent Exits Log)
            var recentExits = await _context.AttendanceRecords
                .Include(a => a.Member)
                .Where(a => a.Date == today && a.CheckOut != null)
                .OrderByDescending(a => a.CheckOut)
                .Take(5)
                .ToListAsync();

            // 4. Liiska xubnaha oo dhan si loogu raadiyo terminal-ka
            var allMembers = await _context.UserProfiles
                .Where(u => u.Role == "Member" || u.Role == "User") // Ku beddel halka ay ku xiran tahay doorka xubnahaaga
                .ToListAsync();

            ViewBag.TodaysCheckIns = todaysCheckIns;
            ViewBag.CurrentlyInsideCount = currentlyInsideCount;
            ViewBag.CurrentlyInsideList = currentlyInsideList;
            ViewBag.RecentExits = recentExits;
            ViewBag.AllMembers = allMembers;

            return View();
        }

        // 5. Action-ka Check-In
        [HttpPost]
        public async Task<IActionResult> ProcessCheckIn(string memberId)
        {
            if (string.IsNullOrEmpty(memberId))
            {
                TempData["Error"] = "Fadlan dooro xubin sax ah!";
                return RedirectToAction(nameof(CheckInOut));
            }

            var today = DateTime.UtcNow.Date;
            var currentTime = DateTime.Now.TimeOfDay;

            // 1. Hubi inuu qofkani HADDA gudaha ku jiro (CheckIn jira laakiin CheckOut ma jiro)
            var alreadyInside = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.MemberId == memberId && a.Date == today && a.CheckOut == null);

            if (alreadyInside != null)
            {
                // Halkaana waa inaysan u oggolaan inuu Check-In labaad sameeyo inta uusan ka bixin
                TempData["Error"] = "Xubnahaani hadda way ku jiraan gym-ka!";
                return RedirectToAction(nameof(CheckInOut));
            }

            // 2. Haddii uusan gudaha ku jirin, si toos ah u samee Check-In (Xitaa haddii uusan horay Check-Out u samaynin maanta)
            var record = new AttendanceRecord
            {
                MemberId = memberId,
                Date = today,
                CheckIn = currentTime,
                Status = "Present",
                ClassName = "General Gym Access"
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CheckInOut));
        }
        // 6. Action-ka Check-Out (Exit)
        [HttpPost]
        public async Task<IActionResult> ProcessCheckOut(string memberId)
        {
            var today = DateTime.UtcNow.Date;
            var currentTime = DateTime.Now.TimeOfDay;

            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.MemberId == memberId && a.Date == today && a.CheckOut == null);

            if (record != null)
            {
                record.CheckOut = currentTime;
                _context.AttendanceRecords.Update(record);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(CheckInOut));
        }
        public async Task<IActionResult> ClassSchedule(string searchString, string dateFilter)
        {
            var classesQuery = _context.GymClasses.AsQueryable();

            // 1. Search filter (by Class Name or Trainer Name)
            if (!string.IsNullOrEmpty(searchString))
            {
                classesQuery = classesQuery.Where(c => c.Name.Contains(searchString) ||
                                                      c.TrainerName.Contains(searchString) ||
                                                      c.Category.Contains(searchString));
            }

            var classesList = await classesQuery.OrderBy(c => c.TimeSlot).ToListAsync();

            // 2. Stat Cards Data (Dynamic calculation)
            ViewBag.TotalClasses = await _context.GymClasses.CountAsync(c => c.Status == "Active");

            // Tusaale ahaan tirada Bookings iyo Available seats (haddii aad leedahay jadwalka booking-ka, halkan waad ku xiri kartaa, hadda waa qiyaas ama xogta tooska ah)
            ViewBag.TotalBookings = 84;
            ViewBag.AvailableSeats = 28;
            ViewBag.FullClasses = await _context.GymClasses.CountAsync(c => c.Capacity <= 0 || c.Status == "Full");

            ViewBag.SearchString = searchString;

            return View(classesList);
        }

        // 3. Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            // 1. STATS CARDS DATA
            var recentUsers = await (from u in _context.Users
                                     join p in _context.UserProfiles on u.Id equals p.Id into profileGroup
                                     from p in profileGroup.DefaultIfEmpty()
                                     orderby u.Id descending
                                     select new
                                     {
                                         Id = u.Id,
                                         Username = u.Name,
                                         Role = u.Role,
                                         Email = p != null ? p.Email : "N/A"
                                     })
                                 .Take(5)
                                 .ToListAsync();

            ViewBag.RecentRegistrationsCount = await _context.Users.CountAsync();
            ViewBag.RecentRegistrations = recentUsers;

            // Today's Payments
            var todayPayments = await _context.Payments
                .Where(p => p.PaymentDate.Date == today)
                .OrderByDescending(p => p.PaymentID)
                .ToListAsync();

            ViewBag.TodayPaymentsList = todayPayments;
            ViewBag.TodayPaymentsTotal = todayPayments.Sum(p => p.AmountPaid);
            ViewBag.TodayPaymentsCount = todayPayments.Count;

            // Visitors Today
            var todayVisitors = await _context.Visitors
                .Where(v => v.Date.Date == today)
                .OrderByDescending(v => v.Id)
                .ToListAsync();

            ViewBag.TodayVisitorsList = todayVisitors;
            ViewBag.TodayVisitorsCount = todayVisitors.Count;

            // 2. EXPIRING SOON / CLASS SCHEDULE / ATTENDANCE
            var attendanceList = await _context.AttendanceRecords
                .Where(a => a.Date.Date == today)
                .OrderByDescending(a => a.Id)
                .Take(5)
                .ToListAsync();
            ViewBag.AttendanceList = attendanceList;

            var classSchedules = await _context.ClassSchedules
                .Take(3)
                .ToListAsync();
            ViewBag.ClassSchedules = classSchedules;

            var reservations = await _context.GymReservations
                .OrderByDescending(r => r.ReservationID)
                .Take(5)
                .ToListAsync();
            ViewBag.Reservations = reservations;

            // 3. CHART DATA (Dynamic Calculations)

            int[] weeklyRegs = new int[7];
            for (int i = 0; i < 7; i++)
            {
                var targetDate = startOfWeek.AddDays(i).Date;
                var nextDate = targetDate.AddDays(1);

                // Tirakoobka oo sax ah
                weeklyRegs[i] = await _context.UserProfiles
                    .CountAsync(p => p.JoinDate >= targetDate && p.JoinDate < nextDate);
            }
            ViewBag.ChartDailyRegs = weeklyRegs;

            // B. Payment Methods Breakdown (Cash vs Card vs Bank)
            var cashCount = await _context.Payments.CountAsync(p => p.Method == "Cash" || p.Method == "cash");
            var cardCount = await _context.Payments.CountAsync(p => p.Method == "Card" || p.Method == "card" || p.Method == "Credit Card");
            var bankCount = await _context.Payments.CountAsync(p => p.Method == "Bank Transfer" || p.Method == "Online");
            ViewBag.PaymentBreakdown = new int[] { cashCount, cardCount, bankCount };

            // C. Membership Plans & Dynamic User Counts per Plan
            var plans = await _context.MembershipPlans
                .Where(m => m.IsActive) // Haddii uu yahay bool saafi ah
                .ToListAsync();

            var planNames = plans.Select(m => m.Name).ToArray();
            var planCounts = new List<int>();

            foreach (var plan in plans)
            {
                // Tirada xubnaha haysta qorshahaan (MembershipType)
                int count = await _context.UserProfiles
                    .CountAsync(u => u.MembershipType == plan.Name || u.MembershipType == plan.ShortCode);
                planCounts.Add(count);
            }

            ViewBag.MembershipPlanNames = planNames;
            ViewBag.MembershipTypesCount = planCounts.ToArray();

            return View();
        }
        // 4. Members
        [HttpGet]
        public async Task<IActionResult> Members(string? searchString, string? statusFilter)
        {
            var membersQuery = _context.UserProfiles
                .Where(u => u.Role == "Member")
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                membersQuery = membersQuery.Where(m =>
                    (m.Name != null && m.Name.Contains(searchString)) ||
                    (m.Id != null && m.Id.Contains(searchString)) ||
                    (m.Phone != null && m.Phone.Contains(searchString)) ||
                    (m.Email != null && m.Email.Contains(searchString)) ||
                    (m.Name != null && m.Name.Contains(searchString))
                );
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
            if (string.IsNullOrEmpty(member.MembershipType))
                ModelState.AddModelError("MembershipType", "Fadlan dooro nuuca xubinnimada.");

            ModelState.Remove("Id");
            ModelState.Remove("Role");
            ModelState.Remove("Status");
            ModelState.Remove("Avatar");
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                // 🟢 1. SI SAX AH U RAADI ID-GA UGU DAMBEEYAY EE JIRA EE KA BILAAWDA "MEM-"
                var lastMember = await _context.UserProfiles
                    .Where(u => u.Id != null && u.Id.StartsWith("MEM-"))
                    .ToListAsync(); // Soo qaad dhammaan si aanu ugu sifayno C# dhankiisa

                int nextIdNum = 1001;

                if (lastMember.Any())
                {
                    // Ka soo saar nambarka ugu sarreeya adigoo ka jaraya "MEM-"
                    var maxId = lastMember
                        .Select(m => {
                            int.TryParse(m.Id.Replace("MEM-", ""), out int num);
                            return num;
                        })
                        .DefaultIfEmpty(1000)
                        .Max();

                    nextIdNum = maxId + 1;
                }

                member.Id = $"MEM-{nextIdNum}";

                // 2. Hubi Email-ka
                if (!string.IsNullOrEmpty(member.Email))
                {
                    bool emailExists = await _context.UserProfiles
                        .AnyAsync(u => u.Email != null && u.Email.ToLower() == member.Email.ToLower());

                    if (emailExists)
                    {
                        TempData["ErrorMessage"] = "Email-kan horay ayaa loo isticmaalay. Fadlan isku day mid kale.";
                        return RedirectToAction(nameof(Members));
                    }
                }
                else
                {
                    member.Email = $"{member.Id.ToLower()}@ironcore.com";
                }

                // 🟢 3. HASH-GAREE PASSWORD-KA MARKA LA ABUURO
                // Haddii uu form-ka ka soo baxo plain text waa la Hash-gareeyaa, haddii kalena waxaa loo sameeyaa default password (sida: Member123!)
                string rawPassword = string.IsNullOrEmpty(member.Password) ? "Member123!" : member.Password;
                member.Password = _passwordHasher.HashPassword(member, rawPassword);

                // 4. Buuxi xogaha hartay ee UserProfile
                member.Role = "Member";
                member.Status = "Active";
                member.JoinDate = DateTime.Now;
                member.LastLogin = DateTime.Now;
                member.Avatar = !string.IsNullOrEmpty(member.Name) && member.Name.Length >= 2
                    ? member.Name.Substring(0, 2).ToUpper()
                    : "ME";

                _context.Add(member);

                // 🟢 5. KU DAR USER MODEL (Users Table)
                var avatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(member.Name ?? "Member")}&background=a3e635&color=000";

                var newUser = new User
                {
                    Id = member.Id,
                    Name = member.Name,
                    Role = "Member",
                    Avatar = avatarUrl,
                    Status = "Offline",
                    LastSeen = DateTime.Now,
                    Specialization = null,
                    MembershipType = member.MembershipType,
                    Shift = null
                };
                _context.Users.Add(newUser);

                _context.Notifications.Add(new Notification
                {
                    TargetRole = "Admin",
                    Title = "New Member Registered",
                    Message = $"A new member {member.Name} ({member.Id}) has been registered by Receptionist.",
                    Type = "Member",
                    IsRead = false,
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "member ka si guul leh ayaa loo diiwaangeliyey!";
                return RedirectToAction(nameof(Members));
            }

            TempData["ErrorMessage"] = "Fadlan hubi xogta aad buuxisay.";
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

            ModelState.Clear();

            existingMember.Name = member.Name;
            existingMember.Gender = member.Gender;
            existingMember.DateOfBirth = member.DateOfBirth;
            existingMember.Phone = member.Phone;
            existingMember.Email = member.Email;
            existingMember.Address = member.Address;
            existingMember.MembershipType = member.MembershipType;
            existingMember.Username = member.Username;
            existingMember.ClassName = member.ClassName; // Halkan lagu cusboonaysiiyay ClassName

            // 🟢 HADDII PASSWORD CUSUB LA SOO GALIYO MARKA LA EDIT-GAYNAYO:
            if (!string.IsNullOrEmpty(member.Password))
            {
                existingMember.Password = _passwordHasher.HashPassword(existingMember, member.Password);
            }

            if (!string.IsNullOrEmpty(member.Name) && member.Name.Length >= 2)
            {
                existingMember.Avatar = member.Name.Substring(0, 2).ToUpper();
            }

            // 🟢 CUSBOONAYSIIN USER TABLE-KA (Users Model)
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser != null)
            {
                existingUser.Name = member.Name;
                existingUser.MembershipType = member.MembershipType;
                existingUser.Avatar = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(member.Name ?? "Member")}&background=a3e635&color=000";
                _context.Users.Update(existingUser);
            }

            try
            {
                _context.Update(existingMember);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xogta xubinta si guul leh ayaa loo beddelay!";
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

        // ================= 4. DELETE MEMBER =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var member = await _context.UserProfiles.FindAsync(id);
            if (member != null && member.Role == "Member")
            {
                _context.UserProfiles.Remove(member);

                // 🟢 TIRTIR KU SHUBIDA USERS TABLE-KA
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xubinta si guul leh ayaa loo tirtiray.";
            }

            return RedirectToAction(nameof(Members));
        }
        // ================= MEMBERSHIP MANAGEMENT & SUBSCRIPTIONS =================
        public async Task<IActionResult> Memberships(string searchString)
        {
            // 1. Hel dhammaan qorshayaasha diiwaangashan (Plans)
            var plans = await _context.MembershipPlans.Where(p => p.IsActive).ToListAsync();
            ViewBag.MembershipPlans = plans;

            // 2. Query-ga xubnaha iyo xogtooda
            var membersQuery = _context.UserProfiles
            .Where(u => u.Role == "Member" || u.Status == "Pending")
            .AsQueryable();

            // 3. Raadinta (Search)
            if (!string.IsNullOrEmpty(searchString))
            {
                membersQuery = membersQuery.Where(m =>
                    (m.Name != null && m.Name.Contains(searchString)) ||
                    (m.Phone != null && m.Phone.Contains(searchString)) ||
                    (m.MembershipType != null && m.MembershipType.Contains(searchString)) ||
                    (m.Id != null && m.Id.Contains(searchString))
                );
            }

            var membersList = await membersQuery.OrderByDescending(m => m.JoinDate).ToListAsync();

            // 4. Stat Cards Dynamic Data
            ViewBag.TotalActive = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.Status == "Active");
            ViewBag.ExpiredCount = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.Status == "Expired");
            ViewBag.PendingCount = await _context.UserProfiles.CountAsync(u => u.Role == "Member" && u.Status == "Pending");

            // Qiyaasta dakhliga bisha (Monthly Revenue)
            ViewBag.MonthlyRevenue = 4850;
            ViewBag.SearchString = searchString;

            return View(membersList);
        }

        // 5. Action-ka Renewal-ka
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRenewal(string MemberId, string NewPlan, string Duration, DateTime StartDate, decimal Amount, string PaymentMethod, string PaymentStatus, string Notes)
        {
            var member = await _context.UserProfiles.FindAsync(MemberId);
            if (member != null)
            {
                member.MembershipType = NewPlan;
                member.Status = "Active";
                // Waad keydin kartaa lacagaha ama taariikhaha haddii aad leedahay table lacag bixin
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xubinnimadii si guul leh ayaa loo cusboonaysiiyey!";
            }
            return RedirectToAction(nameof(Memberships));
        }

        // 6. Action-ka Approve / Reject-ka
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPendingRequest(string MemberId, string actionType)
        {
            var oldProfile = await _context.UserProfiles.FindAsync(MemberId);

            if (oldProfile == null) return NotFound();

            if (actionType == "Approve")
            {
                // 1. Samee ID cusub
                var memberCount = await _context.UserProfiles.CountAsync(p => p.Role == "Member");
                var newMemberId = $"MEM-{memberCount + 1:D3}";

                // 2. Abuur Profile cusub oo wata ID cusub
                var newProfile = new UserProfile
                {
                    Id = newMemberId,
                    Name = oldProfile.Name,
                    Email = oldProfile.Email,
                    Username = oldProfile.Username,
                    Password = oldProfile.Password,
                    Role = "Member",
                    Status = "Active",
                    Avatar = oldProfile.Avatar,

                    // Halkan ku dar wixii ka dhiman ee table-kaagu rabo:
                    Gender = oldProfile.Gender ?? "Not Specified", // Haddii uu NULL ahaa ku beddel qiime default ah
                    Phone = oldProfile.Phone ?? string.Empty,
                    Address = oldProfile.Address ?? string.Empty,
                    DateOfBirth = oldProfile.DateOfBirth ?? string.Empty,
                    Bio = oldProfile.Bio ?? string.Empty,
                    MembershipType = oldProfile.MembershipType ?? string.Empty,
                    Specialization = oldProfile.Specialization ?? string.Empty,
                    Shift = oldProfile.Shift ?? string.Empty,
                    JoinDate = oldProfile.JoinDate,
                    LastLogin = oldProfile.LastLogin,
                    MembershipRequested = false
                };
                // 3. Ku dar Profile-ka cusub, tirtir kii hore
                _context.UserProfiles.Add(newProfile);
                _context.UserProfiles.Remove(oldProfile);

                // 4. Samee isla habkaas miiska Users-ka
                var oldUser = await _context.Users.FindAsync(MemberId);
                if (oldUser != null)
                {
                    var newUser = new User
                    {
                        Id = newMemberId,
                        Name = oldUser.Name,
                        Role = "Member",
                        Avatar = oldUser.Avatar,
                        Status = "Offline"
                        // ... (halkan ku dar field-yada kale)
                    };
                    _context.Users.Add(newUser);
                    _context.Users.Remove(oldUser);
                }

                TempData["SuccessMessage"] = $"Codsiga waa la ogolaaday. ID-gaaga cusub waa {newMemberId}";
            }
            else if (actionType == "Reject")
            {
                oldProfile.Status = "Rejected";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Memberships));
        }
        public async Task<IActionResult> Orders(string searchString, string statusFilter)
        {
            // Waxaan xogta toos uga soo akhrineynaa GuestOrders-kaaga hadda jira
            var ordersQuery = _context.GuestOrders
                .Include(o => o.Items)
                .AsQueryable();

            // 1. Search filter (Magaca, Telefoonka ama Order Number-ka)
            if (!string.IsNullOrEmpty(searchString))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderNumber.Contains(searchString) ||
                                                      o.FullName.Contains(searchString) ||
                                                      o.Phone.Contains(searchString));
            }

            // 2. Status filter (Pending, Confirmed, Cancelled, iwm.)
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                ordersQuery = ordersQuery.Where(o => o.Status.ToLower() == statusFilter.ToLower());
            }

            var ordersList = await ordersQuery.OrderByDescending(o => o.OrderDate).ToListAsync();

            // Tirakoobka Dashboard-ka (Stat Cards)
            ViewBag.TotalOrders = await _context.GuestOrders.CountAsync();
            ViewBag.PendingOrders = await _context.GuestOrders.CountAsync(o => o.Status == "Pending");
            ViewBag.ConfirmedOrders = await _context.GuestOrders.CountAsync(o => o.Status == "Confirmed");
            ViewBag.CompletedOrders = await _context.GuestOrders.CountAsync(o => o.Status == "Completed");

            return View(ordersList);
        }

        // Action-ka Receptionist-ku ku xaqiijinayo (Confirm) ama ku diidayo (Cancel) Dalabka
        // 7. Process Order Action (Confirm / Cancel / Forward to Payments)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(int id, string actionType)
        {
            var order = await _context.GuestOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Dalabka lama helin.";
                return RedirectToAction(nameof(Orders));
            }

            if (actionType == "Confirm")
            {
                order.Status = "Confirmed";
                order.PaymentMethod = "Cash"; // Ama habka lacag-bixinta ee uu doortay

                // Hubi in uusan horay ugu jirin Payments Table-ka si uusan laba jeer u gelin
                bool paymentExists = await _context.Payments
                    .AnyAsync(p => p.Reference == order.OrderNumber);

                if (!paymentExists)
                {
                    // Abuur xogta lacag-bixinta oo u gudbi Payments Desk
                    var payment = new Payment
                    {
                        ReceiptNumber = $"REC-{new Random().Next(1000, 9999)}",
                        Reference = order.OrderNumber,
                        MemberName = order.FullName,
                        PaymentFor = "Store Order / Supplement",
                        Method = order.PaymentMethod ?? "Cash",
                        AmountDue = order.TotalAmount,
                        AmountPaid = order.TotalAmount, // Kadib xaqiijinta waxaa lagu bixiyay lacagtiisa
                        PaymentDate = DateTime.Now,
                        Status = "Paid",
                        Notes = $"Auto-generated from Order: {order.OrderNumber}"
                    };

                    _context.Payments.Add(payment);
                }

                TempData["SuccessMessage"] = "Dalabka waa la xaqiijiyay, xogtuna waxay si guul leh ugu gudubtay Payments Desk!";
            }
            else if (actionType == "Cancel")
            {
                order.Status = "Cancelled";
                TempData["ErrorMessage"] = "Dalabka waa la joojiyay.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Orders));
        }
        public async Task<IActionResult> Payments()
        {
            var paymentsList = await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(paymentsList);
        }

        // 2. POST: Record New Payment (Diiwaangelinta Lacag Cusub)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(Payment payment)
        {
            // Abuurista Receipt Number otomaatig ah haddii uusan jirin (Tusaale: REC-8421)
            if (string.IsNullOrEmpty(payment.ReceiptNumber))
            {
                var random = new Random();
                payment.ReceiptNumber = $"REC-{random.Next(1000, 9999)}";
            }

            // Go'aaminta Status-ka iyadoo la eegayo lacagta la dhiibay iyo midda la rabay
            if (payment.AmountPaid >= payment.AmountDue)
            {
                payment.Status = "Paid";
            }
            else if (payment.AmountPaid > 0 && payment.AmountPaid < payment.AmountDue)
            {
                payment.Status = "Partial";
            }
            else
            {
                payment.Status = "Pending";
            }

            // Hubinta in taariikhdu aysan ahayn madhan
            if (payment.PaymentDate == default)
            {
                payment.PaymentDate = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lacagta si guul leh ayaa loo diiwaangeliyay!";
                return RedirectToAction(nameof(Payments));
            }

            // Haddii uu khalad jiro dib u celi bogga
            TempData["ErrorMessage"] = "Fadlan hubi xogta aad gelisay.";
            var paymentsList = await _context.Payments.OrderByDescending(p => p.PaymentDate).ToListAsync();
            return View("Payments", paymentsList);
        }

        // 8. Report
        // GET: Front Desk Reports
        [HttpGet]
        public async Task<IActionResult> Report(string tab = "attendance")
        {
            ViewBag.CurrentTab = string.IsNullOrEmpty(tab) ? "attendance" : tab;
            var today = DateTime.UtcNow.Date;

            // 1. Attendance Records
            var attendances = await _context.AttendanceRecords
                .Where(a => a.Date.Date == today)
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            ViewBag.AttendanceList = attendances;
            ViewBag.TotalCheckIns = attendances.Count;
            ViewBag.CurrentlyInsideCount = attendances.Count(a => a.Status == "Inside" || a.Status == "Checked In");
            ViewBag.CheckedOutCount = attendances.Count(a => a.Status == "Completed" || a.Status == "Checked Out");

            // 2. Payment Collections Data
            var payments = await _context.Payments
                .Where(p => p.PaymentDate.Date == today)
                .OrderByDescending(p => p.PaymentID)
                .ToListAsync();

            ViewBag.PaymentList = payments;
            ViewBag.TotalCollectedToday = payments.Sum(p => p.AmountPaid);
            ViewBag.PaymentCount = payments.Count;

            // 3. New Members / Users Registrations
            var registrations = await _context.UserProfiles
                .OrderByDescending(u => u.Id)
                .Take(20) // Waxaad ku xiri kartaa taariikhda maanta haddii ay leedahay CreatedDate
                .ToListAsync();

            ViewBag.RegistrationList = registrations;
            ViewBag.NewMembersCount = registrations.Count;

            // 4. Visitor Activity Data
            var visitors = await _context.Visitors
                .Where(v => v.Date.Date == today)
                .OrderByDescending(v => v.Id)
                .ToListAsync();

            ViewBag.VisitorList = visitors;
            ViewBag.VisitorCount = visitors.Count;
            ViewBag.VisitorInsideCount = visitors.Count(v => v.Status == "Inside");

            // 5. Class Reservations Data
            var reservations = await _context.GymReservations
                .OrderByDescending(c => c.ReservationID)
                .ToListAsync();

            ViewBag.ReservationList = reservations;
            ViewBag.ReservationCount = reservations.Count;

            return View();
        }

        [HttpGet]
        public IActionResult ExportReport(string tab)
        {
            TempData["Success"] = $"Report for {tab} exported successfully!";
            return RedirectToAction(nameof(Report), new { tab });
        }

        // 9. Reservation
        // 1. GET: Class Reservations Page
        public async Task<IActionResult> Reservation(string searchString, string statusFilter)
        {
            var reservationsQuery = _context.GymReservations.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                reservationsQuery = reservationsQuery.Where(r => r.ReservationNumber.Contains(searchString) ||
                                                                r.MemberName.Contains(searchString) ||
                                                                r.ClassName.Contains(searchString) ||
                                                                r.TrainerName.Contains(searchString));
            }

            var list = await reservationsQuery.OrderByDescending(r => r.ReservationDate).ToListAsync();

            ViewBag.ClassList = await _context.GymClasses.ToListAsync();
            ViewBag.TrainerList = await _context.UserProfiles
                .Where(u => u.Role == "Trainer") // Beddel magaca sifooyinka (properties) haddii ay ka duwan yihiin (tusaale: u.UserRole)
                .ToListAsync();
            return View(list);
        }

        // 2. POST: Create New Reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(GymReservation reservation)
        {
            var random = new Random();
            reservation.ReservationNumber = $"RES-{random.Next(1000, 9999)}";

            if (string.IsNullOrEmpty(reservation.Status))
            {
                reservation.Status = "Pending";
            }

            _context.GymReservations.Add(reservation);
            _context.Notifications.Add(new Notification
            {
                TargetRole = "Admin",
                Title = "New Class Reservation",
                Message = $"A new reservation ({reservation.ReservationNumber}) was created for class {reservation.ClassName}.",
                Type = "Reservation",
                IsRead = false,
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Boos-qabashadii si guul leh ayaa loo diiwaangeliyey!";
            return RedirectToAction(nameof(Reservation));
        }

        // 3. POST: Process Reservation (Confirm / Cancel)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReservation(int id, string actionType)
        {
            var reservation = await _context.GymReservations.FindAsync(id);
            if (reservation != null)
            {
                if (actionType == "Confirm")
                {
                    reservation.Status = "Confirmed";
                    TempData["SuccessMessage"] = "Boos-qabashada waa la xaqiijiyay.";
                }
                else if (actionType == "Cancel")
                {
                    reservation.Status = "Cancelled";
                    TempData["ErrorMessage"] = "Boos-qabashada waa la joojiyay.";
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reservation));
        }

        // 10. VisitorLog
        [HttpGet]
        public async Task<IActionResult> VisitorLog()
        {
            var today = DateTime.UtcNow.Date;

            var visitors = await _context.Visitors
                .Where(v => v.Date == today)
                .OrderByDescending(v => v.Id)
                .ToListAsync();

            // Xisaabinta Summary Cards-ka
            ViewBag.TotalVisitorsToday = visitors.Count;
            ViewBag.CurrentlyInside = visitors.Count(v => v.Status == "Inside");
            ViewBag.TrialVisitors = visitors.Count(v => v.VisitType == "Trial Workout");
            ViewBag.Inquiries = visitors.Count(v => v.VisitType == "Inquiry");

            return View(visitors);
        }

        // POST: Add New Visitor
        [HttpPost]
        public async Task<IActionResult> AddVisitor(Visitor visitor, string checkInTimeString)
        {
            visitor.Date = DateTime.UtcNow.Date;
            visitor.CheckInTime = DateTime.Now.TimeOfDay;
            visitor.Status = "Inside";

            _context.Visitors.Add(visitor);
            _context.Notifications.Add(new Notification
            {
                TargetRole = "Admin",
                Title = "New Visitor Logged",
                Message = $"Visitor {visitor.FullName} has checked into the gym as a {visitor.VisitType}.",
                Type = "Visitor",
                IsRead = false,
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Visitor registered successfully!";
            return RedirectToAction(nameof(VisitorLog));
        }

        // POST: Checkout Visitor
        [HttpPost]
        public async Task<IActionResult> CheckoutVisitor(int id)
        {
            var visitor = await _context.Visitors.FindAsync(id);
            if (visitor != null && visitor.Status == "Inside")
            {
                visitor.CheckOutTime = DateTime.Now.TimeOfDay;
                visitor.Status = "Completed";
                _context.Visitors.Update(visitor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(VisitorLog));
        }
        [HttpGet]
        public async Task<IActionResult> ContactMessages(string searchString)
        {
            var query = _context.ContactMessages.AsQueryable();

            // Raadinta haddii Admin-ku uu wax ku dhex raadiyo Magaca ama Email-ka
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(m => m.Name.Contains(searchString) ||
                                         m.Email.Contains(searchString) ||
                                         m.Subject.Contains(searchString));
            }

            var messages = await query.OrderByDescending(m => m.SubmittedAt).ToListAsync();

            // Tirakoobka fariimaha (Stat Cards)
            ViewBag.TotalMessages = await _context.ContactMessages.CountAsync();
            ViewBag.UnreadMessages = await _context.ContactMessages.CountAsync(m => !m.IsRead);
            ViewBag.CurrentSearch = searchString;

            return View(messages);
        }

        // Hab lagu calaamadiyo fariinta in la akhriyey (Marka la gujiyo)
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ContactMessages));
        }

        // Hab lagu tirtiro fariinta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContactMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                _context.ContactMessages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Fariinta waa la tirtiray.";
            }
            return RedirectToAction(nameof(ContactMessages));
        }


    }
}