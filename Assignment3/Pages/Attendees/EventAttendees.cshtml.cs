using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using Assignment3.Models;

namespace Assignment3.Pages.Attendees
{
    [Authorize]
    public class EventAttendeesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EventAttendeesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Event? Event { get; set; }
        public List<Attendee> Attendees { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Event = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (Event == null)
            {
                return NotFound();
            }

            Attendees = await _context.Attendees
                .Include(a => a.User)
                .Where(a => a.EventID == id)
                .OrderByDescending(a => a.RegistrationTime)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAttendeeAsync(int attendeeId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("EventManager"))
            {
                return Forbid();
            }

            var attendee = await _context.Attendees.FindAsync(attendeeId);
            if (attendee != null)
            {
                _context.Attendees.Remove(attendee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Attendee removed successfully.";
            }

            return RedirectToPage(new { id = attendee?.EventID });
        }
    }
} 