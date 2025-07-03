using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Assignment3.Models;
using Assignment3.ViewModels;
using Assignment3.Hubs;
using Assignment3.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment3.Controllers
{
    [Authorize]
    public class AttendeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventHub> _hubContext;

        public AttendeesController(ApplicationDbContext context, IHubContext<EventHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Attendees
        public async Task<IActionResult> Index()
        {
            var attendees = await _context.Attendees
                .Include(a => a.Event)
                .Include(a => a.User)
                .OrderByDescending(a => a.RegistrationTime)
                .ToListAsync();

            return View(attendees);
        }

        // GET: Attendees/Register/5
        public async Task<IActionResult> Register(int? id)
        {
            if (id == null) return NotFound();

            var eventModel = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventModel == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isUserRegistered = await _context.Attendees
                .AnyAsync(a => a.EventID == id && a.UserID == userId);

            if (isUserRegistered)
            {
                TempData["Warning"] = "You are already registered for this event.";
                return RedirectToAction("Details", "Events", new { id });
            }

            var user = await _context.Users.FindAsync(userId);
            var viewModel = new EventRegistrationViewModel
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

            return View(viewModel);
        }

        // POST: Attendees/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(EventRegistrationViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                // Check if user is already registered
                var existingRegistration = await _context.Attendees
                    .AnyAsync(a => a.EventID == viewModel.EventID && a.UserID == userId);

                if (existingRegistration)
                {
                    TempData["Warning"] = "You are already registered for this event.";
                    return RedirectToAction("Details", "Events", new { id = viewModel.EventID });
                }

                var attendee = new Attendee
                {
                    EventID = viewModel.EventID,
                    UserID = userId,
                    Name = viewModel.Name,
                    Email = viewModel.Email,
                    RegistrationTime = DateTime.Now
                };

                _context.Attendees.Add(attendee);
                await _context.SaveChangesAsync();

                // SignalR notification
                await _hubContext.Clients.Group($"Event_{viewModel.EventID}")
                    .SendAsync("AttendeeRegistered", viewModel.Name);

                TempData["Success"] = "Successfully registered for the event!";
                return RedirectToAction("Details", "Events", new { id = viewModel.EventID });
            }

            // Reload event data if validation fails
            var eventModel = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.EventID == viewModel.EventID);

            if (eventModel != null)
            {
                viewModel.EventTitle = eventModel.Title;
                viewModel.EventDescription = eventModel.Description ?? "";
                viewModel.StartTime = eventModel.StartTime;
                viewModel.EndTime = eventModel.EndTime;
                viewModel.Location = eventModel.Location;
                viewModel.CategoryName = eventModel.Category?.CategoryName;
                viewModel.AttendeeCount = eventModel.Attendees.Count;
            }

            return View(viewModel);
        }

        // POST: Attendees/Unregister/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unregister(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var attendee = await _context.Attendees
                .FirstOrDefaultAsync(a => a.EventID == eventId && a.UserID == userId);

            if (attendee != null)
            {
                _context.Attendees.Remove(attendee);
                await _context.SaveChangesAsync();

                // SignalR notification
                await _hubContext.Clients.Group($"Event_{eventId}")
                    .SendAsync("AttendeeUnregistered", attendee.Name);

                TempData["Success"] = "Successfully unregistered from the event.";
            }
            else
            {
                TempData["Error"] = "Registration not found.";
            }

            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        // GET: Attendees/EventAttendees/5
        public async Task<IActionResult> EventAttendees(int? id)
        {
            if (id == null) return NotFound();

            var eventModel = await _context.Events
                .Include(e => e.Attendees)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventModel == null) return NotFound();

            ViewBag.EventTitle = eventModel.Title;
            ViewBag.EventId = eventModel.EventID;

            return View(eventModel.Attendees.OrderByDescending(a => a.RegistrationTime));
        }

        // GET: Attendees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var attendee = await _context.Attendees
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AttendeeID == id);

            if (attendee == null) return NotFound();

            return View(attendee);
        }

        // GET: Attendees/Delete/5
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var attendee = await _context.Attendees
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AttendeeID == id);

            if (attendee == null) return NotFound();

            return View(attendee);
        }

        // POST: Attendees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,EventManager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendee = await _context.Attendees.FindAsync(id);
            if (attendee != null)
            {
                _context.Attendees.Remove(attendee);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Attendee removed successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Attendees/MyRegistrations
        public async Task<IActionResult> MyRegistrations()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var myRegistrations = await _context.Attendees
                .Include(a => a.Event)
                .ThenInclude(e => e!.Category)
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.RegistrationTime)
                .ToListAsync();

            return View(myRegistrations);
        }
    }
} 