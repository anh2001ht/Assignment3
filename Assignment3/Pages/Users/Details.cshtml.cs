using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;

namespace Assignment3.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public UserDetailsViewModel UserDetails { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Attendees)
                    .ThenInclude(a => a.Event)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage("./Index");
            }

            var now = DateTime.Now;
            var registeredEvents = user.Attendees.Where(a => a.Event != null).Select(a => a.Event!).ToList();

            UserDetails = new UserDetailsViewModel
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                TotalRegistrations = registeredEvents.Count,
                UpcomingEvents = registeredEvents.Count(e => e.StartTime > now),
                PastEvents = registeredEvents.Count(e => e.StartTime <= now),
                LastActivity = user.Attendees
                    .Where(a => a.RegistrationTime.HasValue)
                    .OrderByDescending(a => a.RegistrationTime)
                    .FirstOrDefault()?.RegistrationTime,
                RegisteredEvents = registeredEvents.Select(e => new EventSummaryViewModel
                {
                    EventID = e.EventID,
                    Title = e.Title,
                    StartTime = e.StartTime ?? DateTime.Now,
                    CategoryName = e.Category?.CategoryName ?? "Uncategorized",
                    AttendeeCount = _context.Attendees.Count(a => a.EventID == e.EventID)
                }).OrderByDescending(e => e.StartTime).ToList()
            };

            return Page();
        }
    }
} 