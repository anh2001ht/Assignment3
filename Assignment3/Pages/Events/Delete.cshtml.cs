using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Assignment3.Models;
using Assignment3.Services;
using Assignment3.Data;
using Assignment3.Hubs;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Pages.Events
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventHub> _hubContext;

        public DeleteModel(IEventService eventService, ApplicationDbContext context, IHubContext<EventHub> hubContext)
        {
            _eventService = eventService;
            _context = context;
            _hubContext = hubContext;
        }

        public Event Event { get; set; } = new();
        public int AttendeeCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var eventModel = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventModel == null)
            {
                return NotFound();
            }

            Event = eventModel;
            AttendeeCount = await _eventService.GetAttendeeCountAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var eventModel = await _eventService.GetEventByIdAsync(id);
            if (eventModel != null)
            {
                await _eventService.DeleteEventAsync(id);

                // SignalR notification
                await _hubContext.Clients.All.SendAsync("EventDeleted", eventModel.Title);

                TempData["Success"] = "Event deleted successfully!";
            }

            return RedirectToPage("/Events/Index");
        }
    }
} 