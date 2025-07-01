using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Assignment3.Models;
using Assignment3.Services;
using Assignment3.Data;
using Assignment3.Hubs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.Pages.Events
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventHub> _hubContext;

        public CreateModel(IEventService eventService, ApplicationDbContext context, IHubContext<EventHub> hubContext)
        {
            _eventService = eventService;
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public EventInputModel Input { get; set; } = new();

        public IList<EventCategory> Categories { get; set; } = new List<EventCategory>();

        public async Task<IActionResult> OnGetAsync()
        {
            Categories = await _context.EventCategories.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Categories = await _context.EventCategories.ToListAsync();

            if (ModelState.IsValid)
            {
                if (Input.EndTime <= Input.StartTime)
                {
                    ModelState.AddModelError("Input.EndTime", "End time must be after start time");
                    return Page();
                }

                var eventModel = new Event
                {
                    Title = Input.Title,
                    Description = Input.Description,
                    StartTime = Input.StartTime,
                    EndTime = Input.EndTime,
                    Location = Input.Location,
                    CategoryID = Input.CategoryID
                };

                await _eventService.CreateEventAsync(eventModel);

                // SignalR notification
                await _hubContext.Clients.All.SendAsync("EventCreated", eventModel.Title);

                TempData["Success"] = "Event created successfully!";
                return RedirectToPage("/Events/Index");
            }

            return Page();
        }

        public class EventInputModel
        {
            [Required]
            [StringLength(200)]
            public string Title { get; set; } = string.Empty;

            [StringLength(1000)]
            public string? Description { get; set; }

            [Required]
            public DateTime StartTime { get; set; }

            [Required]
            public DateTime EndTime { get; set; }

            [StringLength(200)]
            public string? Location { get; set; }

            [Required]
            public int CategoryID { get; set; }
        }
    }
} 