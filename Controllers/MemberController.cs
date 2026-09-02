using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MemberController(ApplicationDbContext context)
        {
            _context = context;
        }
        // 1. Attendance
        public async Task<IActionResult> Attendance(string monthFilter)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "c8d420e6-97e7-4065-a824-9ec0b9d8c3ae";
            string currentUserName = User.Identity?.Name ?? "Amal Ahmet";

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Email == currentUserName || u.Name == currentUserName || u.Email == "amal@gmail.com");

            string memberIdToSearch = userProfile?.Id ?? currentUserId;

            if (string.IsNullOrEmpty(monthFilter))
            {
                monthFilter = DateTime.Now.ToString("yyyy-MM");
            }

            var allLogs = await _context.AttendanceRecords
                .Where(a => a.MemberId == memberIdToSearch || a.MemberId == "c8d420e6-97e7-4065-a824-9ec0b9d8c3ae")
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var filteredLogs = allLogs.Where(a => a.Date.ToString("yyyy-MM") == monthFilter).ToList();

            int totalVisits = filteredLogs.Count(a => a.Status == "Present" || a.Status == "Late");

            var today = DateTime.Today;
            var todayLog = allLogs.FirstOrDefault(a => a.Date.Date == today);
            string todayStatus = "Absent / Not Checked In";
            string checkInTimeStr = "N/A";

            if (todayLog != null)
            {
                todayStatus = todayLog.Status == "Present" ? "Present (In Gym)" : todayLog.Status;

                // Xallinta qaladka: Hubinta nooca CheckIn si uusan System.FormatException u dhicin
                if (todayLog.CheckIn != null)
                {
                    checkInTimeStr = todayLog.CheckIn.ToString(); // Waxay si badbaado leh u soo saaraysaa qoraalka
                }
                else
                {
                    checkInTimeStr = "Checked In";
                }
            }

            int streak = allLogs.Count(a => a.Status == "Present");

            ViewBag.TotalVisits = totalVisits;
            ViewBag.TodayStatus = todayStatus;
            ViewBag.CheckInTime = checkInTimeStr;
            ViewBag.Streak = streak > 0 ? streak : 4;
            ViewBag.SelectedMonth = monthFilter;

            return View(filteredLogs);
        }

        // 2. ClassSchedule
        [Authorize]
        public async Task<IActionResult> ClassSchedule()
        {
            try
            {
                // 1. Hel ID-ga Member-ka hadda logged-in ah (sida Dashboard-ka)
                string currentMemberId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(currentMemberId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // 2. Ka hel xogta user-ka miiska UserProfiles adigoo ka raadinaya ID-ga
                var currentUser = await _context.UserProfiles
                    .FirstOrDefaultAsync(u => u.Id == currentMemberId);

                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                string memberId = currentUser.Id;       // Tusaale: "member1" ama "MEM-1001"
                string memberName = currentUser.Name;   // Tusaale: "Amal Ali"

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                // 3. Soo qaado fasallada uu book-garaystay
                var reservations = await _context.GymReservations
                    .Where(r => r.Status == "Confirmed" &&
                                (r.MemberName.Contains(memberName) || r.MemberName.Contains(memberId)))
                    .OrderBy(r => r.ReservationDate)
                    .ToListAsync();

                // 4. Xisaabi tirada fasallada uu dhameeyay bishan
                var completedSessionsCount = await _context.AttendanceRecords
                    .Where(a => (a.MemberId == memberId || a.MemberId == memberName) &&
                                a.Date.Month == currentMonth &&
                                a.Date.Year == currentYear &&
                                (a.Status == "Present" || a.Status == "Late"))
                    .CountAsync();

                // 5. Xisaabinta Attendance Rate-ka
                int totalScheduledThisMonth = completedSessionsCount + reservations.Count;
                int attendanceRate = totalScheduledThisMonth > 0
                    ? (int)Math.Round((double)completedSessionsCount / totalScheduledThisMonth * 100)
                    : 100;

                // Gudbinta Xogta (ViewBag)
                ViewBag.ReservedCount = reservations.Count;
                ViewBag.CompletedSessions = completedSessionsCount;
                ViewBag.AttendanceRate = attendanceRate;
                ViewBag.CurrentMonthName = DateTime.Now.ToString("MMMM yyyy");

                return View(reservations);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR IN CLASSSCHEDULE: " + ex.Message);
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // 1. Hel ID-ga Member-ka hadda logged-in ah
            string currentMemberId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "member1";

            // 2. Soo hel xogta guud ee Xubinta (Tusaale ahaan UserProfile ama Member Table)
            var memberProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Id == currentMemberId);

            // 3. Soo hel Progress-yadii ugu dambeeyay ee xubintan si Chart-ka iyo Stats-ka loo buuxiyo
            var progressList = await _context.MemberProgresses
                .Where(p => p.MemberId == currentMemberId)
                .OrderBy(p => p.CheckDate)
                .ToListAsync();

            var latestProgress = progressList.LastOrDefault();

            // 4. Soo hel tirada Workouts-ka ama Classes-ka uu book-gareeyay ama dhammaystiray
            int completedWorkoutsCount = progressList.Count;

            // 5. Diyaari xogta Chart-ka (Labels = Taariikhaha, Data = Miisaanka ama dhibcaha progress-ka)
            var chartLabels = progressList.Any()
                ? progressList.Select(p => p.CheckDate.ToString("MMM dd")).ToArray()
                : new[] { "Week 1", "Week 2", "Week 3", "Week 4" };

            var chartData = progressList.Any()
                ? progressList.Select(p => (double)(p.Weight ?? 0)).ToArray()
                : new[] { 0.0, 0.0, 0.0, 0.0 };

            // 6. Soo hel Ogeysiisyada (Notices) haddii ay jiraan, ama liis madhan ka dhig
            var notices = await _context.Notifications
                .OrderByDescending(n => n.Timestamp)
                .Take(3)
                .Select(n => new { Title = n.Title, Date = n.Timestamp.ToString("dd MMM yyyy") })
                .ToListAsync();

            // Model-ka ama ViewBag-ga oo aad ku shubayso xogta
            // Halkan waxaan isticmaaleynaa ViewModel ama ViewBag si aan loogu baahanayn in la sameeyo Model cusub
            ViewBag.MemberName = memberProfile?.Name ?? "Member";
            ViewBag.MembershipStatus = "Active";
            ViewBag.ExpiryDate = "2026-12-31";
            ViewBag.DaysRemaining = 120;
            ViewBag.UpcomingClassesCount = 2;
            ViewBag.WorkoutsCompletedCount = completedWorkoutsCount;

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;
            ViewBag.Notices = notices;

            return View(latestProgress ?? new GymManagementSystem.Models.MemberProgress());
        }
        public async Task<IActionResult> Membership()
        {
            string currentUserName = User.Identity?.Name ?? "Amal Ahmet";

            // 1. Soo qaado dhammaan qorshayaasha gym-ka
            var allPlans = await _context.MembershipPlans.ToListAsync();

            // 2. Soo qaado user-ka hadda soo galay miiska Users/UserProfiles
            var currentUser = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Email == currentUserName || u.Name == currentUserName || u.Email == "amal@gmail.com");

            string activePlanName = currentUser?.MembershipType ?? "No Plan Selected";
            string accountStatus = currentUser?.Status ?? "Active";

            // Xisaabinta taariikhaha iyo maalmaha ka dhiman (Tusaale ahaan 30 maalmood)
            int daysRemaining = 30;
            int progressPercent = 100;
            string startDate = currentUser?.JoinDate.ToString("MMM dd, yyyy") ?? "Aug 16, 2026";
            string expiryDate = currentUser?.JoinDate.AddDays(30).ToString("MMM dd, yyyy") ?? "Sep 15, 2026";

            ViewBag.ActivePlanName = activePlanName;
            ViewBag.AccountStatus = accountStatus;
            ViewBag.StartDate = startDate;
            ViewBag.ExpiryDate = expiryDate;
            ViewBag.DaysRemaining = daysRemaining;
            ViewBag.ProgressPercent = progressPercent;

            return View(allPlans);
        }

        [HttpPost]
        public async Task<IActionResult> SelectPlan(int planId)
        {
            var plan = await _context.MembershipPlans.FindAsync(planId);
            if (plan == null)
            {
                TempData["ErrorMessage"] = "Qorshaha la doortay lama helin.";
                return RedirectToAction(nameof(Membership));
            }

            string currentUserName = User.Identity?.Name ?? "amal@gmail.com";

            // Soo hel user-ka miiska UserProfiles
            var currentUser = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Email == currentUserName || u.Email == "amal@gmail.com");

            if (currentUser != null)
            {
                currentUser.MembershipType = plan.Name; // Isticmaal plan.PlanName (haddii sidaas uu model-ku yahay)

                // Haddii aad rabto in xaaladdu noqoto mid sugaysa approval-ka Admin-ka:
                // currentUser.Status = "Pending"; 

                // EF Core wuu ogyahay isbedelka, laakiin waxaad isticmaali kartaa tan:
                _context.UserProfiles.Update(currentUser);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Guul! Waxaad dooratay qorshaha {plan.Name}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lama helin xogta xubinta ee hadda jirta.";
            }

            return RedirectToAction(nameof(Membership));
        }
        // 5. NutritionPlans
        public async Task<IActionResult> NutritionPlans()
        {
            // 1. Hel magaca ama ID-ga user-ka hadda soo galay
            string currentUserName = User.Identity?.Name ?? "Amal Ahmet";

            // Soo hel user-ka miiska UserProfiles ama Users si loo hesho ID-giisa
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.Email == currentUserName || u.Name == currentUserName || u.Email == "amal@gmail.com");

            string memberId = userProfile?.Id;

            // 2. Raadi qorshaha cunto ee loo assign-gareeyay user-kan (AssignedMemberId)
            var nutritionPlan = await _context.NutritionPlans
                .FirstOrDefaultAsync(p => p.AssignedMemberId == memberId);

            // Haddii uusan jirin qorshe gaar ah oo loo qoondeeyay, soo qaado kan ugu dambeeyay ee guud (default)
            if (nutritionPlan == null)
            {
                nutritionPlan = await _context.NutritionPlans
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            // Gudbinta xogta adigoo isticmaalaya ViewBag ama Model
            ViewBag.NutritionPlan = nutritionPlan;

            return View(nutritionPlan);
        }


        [HttpGet]
        public async Task<IActionResult> Progress()
        {

            string currentMemberId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "member1"; // Default-ka wuxuu u isticmaalayaa tusaalihii member1

            var progressList = await _context.MemberProgresses
                .Where(p => p.MemberId == currentMemberId)
                .OrderByDescending(p => p.CheckDate)
                .ToListAsync();

            var latestProgress = progressList.FirstOrDefault() ?? new MemberProgress
            {
                Weight = 0,
                BodyFat = 0,
                Chest = 0,
                Waist = 0,
                Arms = 0,
                Notes = "Xog weli lama gelin.",
                Goal = "General Fitness",
                TargetWeight = 70,
                CheckDate = DateTime.Now,
                NextCheckDate = DateTime.Now.AddDays(14)
            };

            ViewBag.ProgressList = progressList;
            return View(latestProgress);
        }

        // 7. WorkoutPlans
        [HttpGet]
        public async Task<IActionResult> WorkoutPlans()
        {
            // Hel ID-ga Member-ka logged-in ah
            string currentMemberId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "member1";

            // Ka soo saar database-ka Workout Plan-ka loo assigned-gareeyay member-kan
            var workoutPlan = await _context.WorkoutPlans
                .FirstOrDefaultAsync(p => p.AssignedMemberId == currentMemberId);

            if (workoutPlan == null)
            {
                workoutPlan = new WorkoutPlan
                {
                    Title = "No Workout Plan Assigned Yet",
                    TargetGoal = "N/A",
                    DifficultyLevel = "N/A",
                    Notes = "Fadlan la xiriir macallinkaaga (Trainer) si uu kuu soo geliyo Workout Plan.",
                    ExercisesJson = "[]"
                };
            }
            else if (!string.IsNullOrEmpty(workoutPlan.ExercisesJson) && !workoutPlan.ExercisesJson.StartsWith("["))
            {
                // 1. Kala bixi IDs-ka ka soo baxa string-ka "3,4"
                var exerciseIds = workoutPlan.ExercisesJson
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : 0)
                    .Where(id => id > 0)
                    .ToList();

                // 2. Ka soo saar Database-ka details-ka saraakiisha exercise-yadaas
                // Waxaan ku xirnay columns-ka saxda ah: PrimaryMuscle, Equipment, Difficulty, ThumbnailPath
                var exercises = await _context.Exercises
                    .Where(e => exerciseIds.Contains(e.Id))
                    .Select(e => new
                    {
                        Name = e.Name,
                        Target = e.PrimaryMuscle ?? "General", // PrimaryMuscle halkii TargetGoal ka noqon lahaa
                        Equipment = e.Equipment,
                        Difficulty = e.Difficulty,
                        Thumbnail = e.ThumbnailPath,
                        Sets = 3,
                        Reps = 10,
                        Rest = "60"
                    })
                    .ToListAsync();

                // 3. U baddal JSON Array buuxa oo uu Javascript View-ka ku fahmo
                workoutPlan.ExercisesJson = System.Text.Json.JsonSerializer.Serialize(exercises);
            }

            return View(workoutPlan);
        }
    }
}