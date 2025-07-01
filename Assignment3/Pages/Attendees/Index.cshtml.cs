using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Assignment3.Models;
using Assignment3.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Pages.Attendees
{
    [Authorize(Roles = "Admin,EventManager")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Attendee> Attendees { get; set; } = new List<Attendee>();
        public IList<Event> Events { get; set; } = new List<Event>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int? EventFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DateFilter { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Get all events for filter dropdown
            Events = await _context.Events
                .OrderBy(e => e.Title)
                .ToListAsync();

            // Start with all attendees
            var query = _context.Attendees
                .Include(a => a.Event)
                .ThenInclude(e => e!.Category)
                .Include(a => a.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a => 
                    a.Name.Contains(SearchTerm) ||
                    a.Email.Contains(SearchTerm) ||
                    (a.Event != null && a.Event.Title.Contains(SearchTerm)) ||
                    (a.User != null && a.User.Username.Contains(SearchTerm)));
            }

            // Apply event filter
            if (EventFilter.HasValue)
            {
                query = query.Where(a => a.EventID == EventFilter.Value);
            }

            // Apply date filter
            if (!string.IsNullOrEmpty(DateFilter))
            {
                var now = DateTime.Now;
                switch (DateFilter.ToLower())
                {
                    case "today":
                        query = query.Where(a => a.RegistrationTime.HasValue && 
                                                a.RegistrationTime.Value.Date == now.Date);
                        break;
                    case "week":
                        var weekStart = now.AddDays(-(int)now.DayOfWeek);
                        query = query.Where(a => a.RegistrationTime.HasValue && 
                                                a.RegistrationTime.Value >= weekStart);
                        break;
                    case "month":
                        var monthStart = new DateTime(now.Year, now.Month, 1);
                        query = query.Where(a => a.RegistrationTime.HasValue && 
                                                a.RegistrationTime.Value >= monthStart);
                        break;
                }
            }

            Attendees = await query
                .OrderByDescending(a => a.RegistrationTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var attendee = await _context.Attendees.FindAsync(id);
            
            if (attendee != null)
            {
                _context.Attendees.Remove(attendee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Attendee removed successfully.";
            }
            else
            {
                TempData["Error"] = "Attendee not found.";
            }

            return RedirectToPage();
        }
    }
} 