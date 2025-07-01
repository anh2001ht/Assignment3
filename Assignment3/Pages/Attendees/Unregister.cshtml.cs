using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Assignment3.Models;
using Assignment3.Data;
using Assignment3.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment3.Pages.Attendees
{
    [Authorize]
    public class UnregisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventHub> _hubContext;

        public UnregisterModel(ApplicationDbContext context, IHubContext<EventHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public Event Event { get; set; } = new();
        public Attendee Attendee { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var attendee = await _context.Attendees
                .Include(a => a.Event)
                .ThenInclude(e => e!.Category)
                .FirstOrDefaultAsync(a => a.EventID == id && a.UserID == userId);

            if (attendee == null || attendee.Event == null)
            {
                TempData["Error"] = "Registration not found or you are not registered for this event.";
                return RedirectToPage("/Events/Details", new { id });
            }

            Event = attendee.Event;
            Attendee = attendee;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var attendee = await _context.Attendees
                .Include(a => a.Event)
                .FirstOrDefaultAsync(a => a.EventID == id && a.UserID == userId);

            if (attendee != null)
            {
                var attendeeName = attendee.Name;
                _context.Attendees.Remove(attendee);
                await _context.SaveChangesAsync();

                // SignalR notification
                await _hubContext.Clients.Group($"Event_{id}")
                    .SendAsync("AttendeeUnregistered", attendeeName);

                TempData["Success"] = "Successfully canceled your registration for this event.";
                return RedirectToPage("/Events/Details", new { id });
            }
            else
            {
                TempData["Error"] = "Registration not found.";
                return RedirectToPage("/Events/Details", new { id });
            }
        }
    }
} 