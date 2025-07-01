using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Assignment3.Models;
using Assignment3.Data;
using Assignment3.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.Pages.Attendees
{
    [Authorize]
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventHub> _hubContext;

        public RegisterModel(ApplicationDbContext context, IHubContext<EventHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public RegistrationInputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var eventModel = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventModel == null)
                return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isUserRegistered = await _context.Attendees
                .AnyAsync(a => a.EventID == id && a.UserID == userId);

            if (isUserRegistered)
            {
                TempData["Warning"] = "You are already registered for this event.";
                return RedirectToPage("/Events/Details", new { id });
            }

            var user = await _context.Users.FindAsync(userId);
            
            Input = new RegistrationInputModel
            {
                EventID = eventModel.EventID,
                EventTitle = eventModel.Title,
                EventDescription = eventModel.Description ?? "",
                StartTime = eventModel.StartTime,
                EndTime = eventModel.EndTime,
                Location = eventModel.Location,
                CategoryName = eventModel.Category?.CategoryName,
                AttendeeCount = eventModel.Attendees.Count,
                Name = user?.FullName ?? "",
                Email = user?.Email ?? ""
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var eventModel = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventModel == null)
                return NotFound();

            // Reload event info
            Input.EventID = eventModel.EventID;
            Input.EventTitle = eventModel.Title;
            Input.EventDescription = eventModel.Description ?? "";
            Input.StartTime = eventModel.StartTime;
            Input.EndTime = eventModel.EndTime;
            Input.Location = eventModel.Location;
            Input.CategoryName = eventModel.Category?.CategoryName;
            Input.AttendeeCount = eventModel.Attendees.Count;

            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                // Check if user is already registered
                var existingRegistration = await _context.Attendees
                    .AnyAsync(a => a.EventID == id && a.UserID == userId);

                if (existingRegistration)
                {
                    TempData["Warning"] = "You are already registered for this event.";
                    return RedirectToPage("/Events/Details", new { id });
                }

                var attendee = new Attendee
                {
                    EventID = id,
                    UserID = userId,
                    Name = Input.Name,
                    Email = Input.Email,
                    RegistrationTime = DateTime.Now
                };

                _context.Attendees.Add(attendee);
                await _context.SaveChangesAsync();

                // SignalR notification
                await _hubContext.Clients.Group($"Event_{id}")
                    .SendAsync("AttendeeRegistered", Input.Name);

                TempData["Success"] = "Successfully registered for the event!";
                return RedirectToPage("/Events/Details", new { id });
            }

            return Page();
        }

        public class RegistrationInputModel
        {
            public int EventID { get; set; }
            public string EventTitle { get; set; } = string.Empty;
            public string EventDescription { get; set; } = string.Empty;
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string? Location { get; set; }
            public string? CategoryName { get; set; }
            public int AttendeeCount { get; set; }

            [Required(ErrorMessage = "Name is required")]
            [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
            public string Email { get; set; } = string.Empty;
        }
    }
} 