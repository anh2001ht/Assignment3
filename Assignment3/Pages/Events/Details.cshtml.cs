using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Assignment3.Models;
using Assignment3.Services;
using Assignment3.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment3.Pages.Events
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;

        public DetailsModel(IEventService eventService, ApplicationDbContext context)
        {
            _eventService = eventService;
            _context = context;
        }

        public Event Event { get; set; } = new();
        public bool IsUserRegistered { get; set; }
        public int AttendeeCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var eventModel = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventModel == null)
            {
                return NotFound();
            }

            Event = eventModel;
            AttendeeCount = Event.Attendees.Count;

            // Check if current user is registered for this event
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    IsUserRegistered = await _context.Attendees
                        .AnyAsync(a => a.EventID == id && a.UserID == userId);
                }
            }

            return Page();
        }
    }
} 