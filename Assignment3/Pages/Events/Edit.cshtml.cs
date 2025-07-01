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
    public class EditModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventHub> _hubContext;

        public EditModel(IEventService eventService, ApplicationDbContext context, IHubContext<EventHub> hubContext)
        {
            _eventService = eventService;
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public EventInputModel Input { get; set; } = new();

        public int EventID { get; set; }
        public IList<EventCategory> Categories { get; set; } = new List<EventCategory>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var eventModel = await _eventService.GetEventByIdAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }

            EventID = id;
            Categories = await _context.EventCategories.ToListAsync();

            Input = new EventInputModel
            {
                Title = eventModel.Title,
                Description = eventModel.Description,
                StartTime = eventModel.StartTime ?? DateTime.Now,
                EndTime = eventModel.EndTime ?? DateTime.Now.AddHours(2),
                Location = eventModel.Location,
                CategoryID = eventModel.CategoryID ?? 0
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            EventID = id;
            Categories = await _context.EventCategories.ToListAsync();

            if (ModelState.IsValid)
            {
                if (Input.EndTime <= Input.StartTime)
                {
                    ModelState.AddModelError("Input.EndTime", "End time must be after start time");
                    return Page();
                }

                try
                {
                    var eventModel = new Event
                    {
                        EventID = id,
                        Title = Input.Title,
                        Description = Input.Description,
                        StartTime = Input.StartTime,
                        EndTime = Input.EndTime,
                        Location = Input.Location,
                        CategoryID = Input.CategoryID
                    };

                    await _eventService.UpdateEventAsync(eventModel);

                    // SignalR notification
                    await _hubContext.Clients.Group($"Event_{id}").SendAsync("EventUpdated", "Event details have been updated");

                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToPage("/Events/Index");
                }
                catch (Exception)
                {
                    var exists = await _eventService.GetEventByIdAsync(id);
                    if (exists == null)
                        return NotFound();
                    else
                        throw;
                }
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