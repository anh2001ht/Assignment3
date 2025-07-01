using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Pages.Reports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Overview Statistics
        public int TotalEvents { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRegistrations { get; set; }
        public int ActiveEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int EventManagers { get; set; }
        public int RegistrationsThisMonth { get; set; }
        public int RegistrationsThisWeek { get; set; }
        public string MostPopularCategory { get; set; } = "N/A";
        public decimal AverageRegistrationsPerEvent { get; set; }

        // Filter Properties
        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var now = DateTime.Now;

            // Basic Statistics
            TotalEvents = await _context.Events.CountAsync();
            TotalUsers = await _context.Users.CountAsync();
            TotalRegistrations = await _context.Attendees.CountAsync();

            // Event Statistics
            ActiveEvents = await _context.Events
                .CountAsync(e => e.StartTime.HasValue && e.StartTime.Value > now);
            UpcomingEvents = ActiveEvents; // Same as active events
            PastEvents = await _context.Events
                .CountAsync(e => e.StartTime.HasValue && e.StartTime.Value <= now);

            // User Statistics
            ActiveUsers = await _context.Users
                .CountAsync(u => u.Attendees.Any());
            AdminUsers = await _context.Users
                .CountAsync(u => u.Role == "Admin");
            EventManagers = await _context.Users
                .CountAsync(u => u.Role == "EventManager");

            // Registration Statistics
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            RegistrationsThisMonth = await _context.Attendees
                .CountAsync(a => a.RegistrationTime.HasValue && 
                           a.RegistrationTime.Value >= thisMonthStart);

            var thisWeekStart = now.AddDays(-(int)now.DayOfWeek);
            RegistrationsThisWeek = await _context.Attendees
                .CountAsync(a => a.RegistrationTime.HasValue && 
                           a.RegistrationTime.Value >= thisWeekStart);

            // Average registrations per event
            if (TotalEvents > 0)
            {
                AverageRegistrationsPerEvent = Math.Round((decimal)TotalRegistrations / TotalEvents, 1);
            }

            // Most popular category
            var categoryStats = await _context.EventCategories
                .Include(c => c.Events)
                .ThenInclude(e => e.Attendees)
                .Select(c => new { 
                    Name = c.CategoryName, 
                    Count = c.Events.SelectMany(e => e.Attendees).Count() 
                })
                .OrderByDescending(c => c.Count)
                .FirstOrDefaultAsync();

            MostPopularCategory = categoryStats?.Name ?? "N/A";
        }
    }

    // Extension method for string formatting
    public static class StringExtensions
    {
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
} 