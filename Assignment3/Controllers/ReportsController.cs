using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Assignment3.Data;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Assignment3.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports (Default Index action)
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Reports/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = new ReportDashboardViewModel
            {
                TotalEvents = await _context.Events.CountAsync(),
                TotalAttendees = await _context.Attendees.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalCategories = await _context.EventCategories.CountAsync(),

                UpcomingEvents = await _context.Events
                    .Where(e => e.StartTime > DateTime.Now)
                    .CountAsync(),

                PastEvents = await _context.Events
                    .Where(e => e.StartTime <= DateTime.Now)
                    .CountAsync(),

                EventsThisMonth = await _context.Events
                    .Where(e => e.StartTime.HasValue && e.StartTime.Value.Month == DateTime.Now.Month && e.StartTime.Value.Year == DateTime.Now.Year)
                    .CountAsync(),

                RegistrationsThisMonth = await _context.Attendees
                    .Where(a => a.RegistrationTime.HasValue && a.RegistrationTime.Value.Month == DateTime.Now.Month && a.RegistrationTime.Value.Year == DateTime.Now.Year)
                    .CountAsync(),

                // Category distribution
                CategoryStats = await _context.EventCategories
                    .Select(c => new CategoryStatViewModel
                    {
                        CategoryName = c.CategoryName,
                        EventCount = c.Events.Count,
                        TotalAttendees = c.Events.SelectMany(e => e.Attendees).Count()
                    })
                    .OrderByDescending(cs => cs.EventCount)
                    .ToListAsync(),

                // Recent events
                RecentEvents = await _context.Events
                    .Include(e => e.Category)
                    .Include(e => e.Attendees)
                    .OrderByDescending(e => e.StartTime)
                    .Take(5)
                    .Select(e => new EventSummaryViewModel
                    {
                        EventID = e.EventID,
                        Title = e.Title,
                        StartTime = e.StartTime ?? DateTime.MinValue,
                        CategoryName = e.Category.CategoryName,
                        AttendeeCount = e.Attendees.Count
                    })
                    .ToListAsync(),

                // Monthly registration trend (last 6 months)
                MonthlyStats = await GetMonthlyRegistrationStats()
            };

            // Average attendees per event
            var eventsWithAttendees = await _context.Events
                .Include(e => e.Attendees)
                .Where(e => e.Attendees.Any())
                .ToListAsync();

            dashboard.AverageAttendeesPerEvent = eventsWithAttendees.Any() 
                ? eventsWithAttendees.Average(e => e.Attendees.Count) 
                : 0;

            return View(dashboard);
        }

        // GET: Reports/EventAnalytics
        public async Task<IActionResult> EventAnalytics()
        {
            var analytics = new EventAnalyticsViewModel
            {
                EventPerformance = await _context.Events
                    .Include(e => e.Category)
                    .Include(e => e.Attendees)
                    .Where(e => e.StartTime.HasValue)
                    .Select(e => new EventPerformanceViewModel
                    {
                        EventID = e.EventID,
                        EventTitle = e.Title,
                        StartTime = e.StartTime!.Value,
                        CategoryName = e.Category.CategoryName,
                        AttendeeCount = e.Attendees.Count,
                        RegistrationRate = e.StartTime.Value > DateTime.Now ? 
                            (e.Attendees.Count > 0 ? (double)e.Attendees.Count / Math.Max(1, (DateTime.Now - e.StartTime.Value).Days) : 0) : 0
                    })
                    .OrderByDescending(e => e.AttendeeCount)
                    .ToListAsync(),

                PopularTimeSlots = await GetPopularTimeSlots(),
                AttendanceByCategory = await GetAttendanceByCategory()
            };

            return View(analytics);
        }

        // GET: Reports/UserActivity
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserActivity()
        {
            var userActivity = new UserActivityViewModel
            {
                ActiveUsers = await _context.Users
                    .Include(u => u.Attendees)
                    .Select(u => new UserActivitySummaryViewModel
                    {
                        UserID = u.UserID,
                        UserName = u.UserName,
                        Email = u.Email,
                        TotalRegistrations = u.Attendees.Count,
                        LastRegistration = u.Attendees.OrderByDescending(a => a.RegistrationTime).FirstOrDefault().RegistrationTime,
                        UpcomingEvents = u.Attendees.Count(a => a.Event.StartTime.HasValue && a.Event.StartTime.Value > DateTime.Now)
                    })
                    .OrderByDescending(u => u.TotalRegistrations)
                    .ToListAsync(),

                RegistrationTrends = await GetUserRegistrationTrends()
            };

            return View(userActivity);
        }

        // GET: Reports/EventReport
        public async Task<IActionResult> EventReport()
        {
            var events = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .Where(e => e.StartTime.HasValue && e.EndTime.HasValue)
                .OrderByDescending(e => e.StartTime)
                .Select(e => new EventReportViewModel
                {
                    EventID = e.EventID,
                    Title = e.Title,
                    CategoryName = e.Category != null ? e.Category.CategoryName : "No Category",
                    StartTime = e.StartTime!.Value,
                    EndTime = e.EndTime!.Value,
                    Location = e.Location ?? "",
                    AttendeeCount = e.Attendees.Count,
                    Status = e.EndTime.Value < DateTime.Now ? "Completed" :
                            e.StartTime.Value <= DateTime.Now && e.EndTime.Value >= DateTime.Now ? "Ongoing" : "Upcoming"
                })
                .ToListAsync();

            return View(events);
        }

        // GET: Reports/AttendeeReport
        public async Task<IActionResult> AttendeeReport()
        {
            var attendees = await _context.Attendees
                .Include(a => a.Event)
                .Include(a => a.User)
                .Where(a => a.Event != null && a.Event.StartTime.HasValue && a.RegistrationTime.HasValue)
                .OrderByDescending(a => a.RegistrationTime)
                .Select(a => new AttendeeReportViewModel
                {
                    AttendeeName = a.Name ?? "",
                    AttendeeEmail = a.Email ?? "",
                    EventTitle = a.Event!.Title,
                    EventDate = a.Event.StartTime!.Value,
                    RegistrationTime = a.RegistrationTime!.Value,
                    UserName = a.User != null ? a.User.FullName : ""
                })
                .ToListAsync();

            return View(attendees);
        }

        // GET: Reports/CategoryReport
        public async Task<IActionResult> CategoryReport()
        {
            var categories = await _context.EventCategories
                .Include(c => c.Events)
                .ThenInclude(e => e.Attendees)
                .Select(c => new CategoryReportViewModel
                {
                    CategoryName = c.CategoryName,
                    EventCount = c.Events.Count,
                    TotalAttendees = c.Events.Sum(e => e.Attendees.Count),
                    UpcomingEvents = c.Events.Count(e => e.StartTime.HasValue && e.StartTime.Value > DateTime.Now),
                    PastEvents = c.Events.Count(e => e.EndTime.HasValue && e.EndTime.Value < DateTime.Now)
                })
                .OrderByDescending(c => c.EventCount)
                .ToListAsync();

            return View(categories);
        }

        // GET: Reports/ExportEventReport
        public async Task<IActionResult> ExportEventReport()
        {
            var events = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();

            var csv = "Title,Category,Start Time,End Time,Location,Attendee Count,Status\n";
            foreach (var e in events)
            {
                var status = e.EndTime < DateTime.Now ? "Completed" :
                           e.StartTime <= DateTime.Now && e.EndTime >= DateTime.Now ? "Ongoing" : "Upcoming";
                csv += $"\"{e.Title}\",\"{e.Category?.CategoryName}\",\"{e.StartTime:yyyy-MM-dd HH:mm}\",\"{e.EndTime:yyyy-MM-dd HH:mm}\",\"{e.Location}\",{e.Attendees.Count},\"{status}\"\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"EventReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Reports/ExportAttendeeReport
        public async Task<IActionResult> ExportAttendeeReport()
        {
            var attendees = await _context.Attendees
                .Include(a => a.Event)
                .Include(a => a.User)
                .OrderByDescending(a => a.RegistrationTime)
                .ToListAsync();

            var csv = "Attendee Name,Email,Event Title,Event Date,Registration Time,User\n";
            foreach (var a in attendees)
            {
                csv += $"\"{a.Name}\",\"{a.Email}\",\"{a.Event?.Title}\",\"{a.Event?.StartTime:yyyy-MM-dd HH:mm}\",\"{a.RegistrationTime:yyyy-MM-dd HH:mm}\",\"{a.User?.FullName}\"\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"AttendeeReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // Helper methods
        private async Task<List<MonthlyStatViewModel>> GetMonthlyRegistrationStats()
        {
            var stats = new List<MonthlyStatViewModel>();
            
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var eventsCount = await _context.Events
                    .Where(e => e.StartTime.HasValue && e.StartTime.Value.Month == targetDate.Month && e.StartTime.Value.Year == targetDate.Year)
                    .CountAsync();
                
                var registrationsCount = await _context.Attendees
                    .Where(a => a.RegistrationTime.HasValue && a.RegistrationTime.Value.Month == targetDate.Month && a.RegistrationTime.Value.Year == targetDate.Year)
                    .CountAsync();

                stats.Add(new MonthlyStatViewModel
                {
                    Month = targetDate.ToString("MMM yyyy"),
                    EventsCount = eventsCount,
                    RegistrationsCount = registrationsCount
                });
            }

            return stats;
        }

        private async Task<List<TimeSlotPopularityViewModel>> GetPopularTimeSlots()
        {
            var timeSlots = await _context.Events
                .Include(e => e.Attendees)
                .Where(e => e.StartTime.HasValue)
                .GroupBy(e => e.StartTime.Value.Hour)
                .Select(g => new TimeSlotPopularityViewModel
                {
                    Hour = g.Key,
                    EventCount = g.Count(),
                    TotalAttendees = g.SelectMany(e => e.Attendees).Count()
                })
                .OrderBy(ts => ts.Hour)
                .ToListAsync();

            return timeSlots;
        }

        private async Task<List<CategoryAttendanceViewModel>> GetAttendanceByCategory()
        {
            return await _context.EventCategories
                .Include(c => c.Events)
                .ThenInclude(e => e.Attendees)
                .Select(c => new CategoryAttendanceViewModel
                {
                    CategoryName = c.CategoryName,
                    EventCount = c.Events.Count,
                    TotalAttendees = c.Events.SelectMany(e => e.Attendees).Count(),
                    AverageAttendeesPerEvent = c.Events.Any() ? 
                        c.Events.Average(e => e.Attendees.Count) : 0
                })
                .OrderByDescending(c => c.TotalAttendees)
                .ToListAsync();
        }

        private async Task<List<MonthlyStatViewModel>> GetUserRegistrationTrends()
        {
            var trends = new List<MonthlyStatViewModel>();
            
            for (int i = 11; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                
                // Get users who made their first registration in this month
                var newUsers = await _context.Users
                    .Where(u => u.Attendees.Any() && 
                               u.Attendees.OrderBy(a => a.RegistrationTime).First().RegistrationTime.HasValue &&
                               u.Attendees.OrderBy(a => a.RegistrationTime).First().RegistrationTime.Value.Month == targetDate.Month && 
                               u.Attendees.OrderBy(a => a.RegistrationTime).First().RegistrationTime.Value.Year == targetDate.Year)
                    .CountAsync();
                
                var activeUsers = await _context.Attendees
                    .Where(a => a.RegistrationTime.HasValue && a.RegistrationTime.Value.Month == targetDate.Month && a.RegistrationTime.Value.Year == targetDate.Year)
                    .Select(a => a.UserID)
                    .Distinct()
                    .CountAsync();

                trends.Add(new MonthlyStatViewModel
                {
                    Month = targetDate.ToString("MMM yyyy"),
                    EventsCount = newUsers, // Using EventsCount field for new users
                    RegistrationsCount = activeUsers
                });
            }

            return trends;
        }
    }
} 