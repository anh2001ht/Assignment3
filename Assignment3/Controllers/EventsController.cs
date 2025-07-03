using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Assignment3.Models;
using Assignment3.Services;
using Assignment3.ViewModels;
using Assignment3.Hubs;
using Assignment3.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment3.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<EventHub> _hubContext;

        public EventsController(IEventService eventService, ApplicationDbContext context, IHubContext<EventHub> hubContext)
        {
            _eventService = eventService;
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Events
        public async Task<IActionResult> Index(EventSearchViewModel searchModel)
        {
            if (searchModel.SearchTerm != null || searchModel.CategoryId.HasValue || 
                searchModel.StartDate.HasValue || searchModel.EndDate.HasValue || 
                !string.IsNullOrEmpty(searchModel.Location))
            {
                searchModel.Events = await _eventService.SearchEventsAsync(searchModel);
            }
            else
            {
                searchModel.Events = await _eventService.GetAllEventsAsync();
            }

            searchModel.Categories = await _context.EventCategories.ToListAsync();
            return View(searchModel);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var eventModel = await _eventService.GetEventByIdAsync(id.Value);
            if (eventModel == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var viewModel = new EventViewModel
            {
                EventID = eventModel.EventID,
                Title = eventModel.Title,
                Description = eventModel.Description,
                StartTime = eventModel.StartTime,
                EndTime = eventModel.EndTime,
                Location = eventModel.Location,
                CategoryID = eventModel.CategoryID,
                CategoryName = eventModel.Category?.CategoryName,
                AttendeeCount = await _eventService.GetAttendeeCountAsync(eventModel.EventID),
                IsUserRegistered = await _eventService.IsUserRegisteredAsync(eventModel.EventID, userId)
            };

            return View(viewModel);
        }

        // GET: Events/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new EventViewModel
            {
                Categories = await _context.EventCategories.ToListAsync(),
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(1).AddHours(2)
            };
            return View(viewModel);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.EndTime <= viewModel.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    viewModel.Categories = await _context.EventCategories.ToListAsync();
                    return View(viewModel);
                }

                var eventModel = new Event
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    StartTime = viewModel.StartTime,
                    EndTime = viewModel.EndTime,
                    Location = viewModel.Location,
                    CategoryID = viewModel.CategoryID
                };

                await _eventService.CreateEventAsync(eventModel);

                // SignalR notification
                await _hubContext.Clients.All.SendAsync("EventCreated", eventModel.Title);

                TempData["Success"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            viewModel.Categories = await _context.EventCategories.ToListAsync();
            return View(viewModel);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var eventModel = await _eventService.GetEventByIdAsync(id.Value);
            if (eventModel == null) return NotFound();

            var viewModel = new EventViewModel
            {
                EventID = eventModel.EventID,
                Title = eventModel.Title,
                Description = eventModel.Description,
                StartTime = eventModel.StartTime,
                EndTime = eventModel.EndTime,
                Location = eventModel.Location,
                CategoryID = eventModel.CategoryID,
                Categories = await _context.EventCategories.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel viewModel)
        {
            if (id != viewModel.EventID) return NotFound();

            if (ModelState.IsValid)
            {
                if (viewModel.EndTime <= viewModel.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    viewModel.Categories = await _context.EventCategories.ToListAsync();
                    return View(viewModel);
                }

                try
                {
                    var eventModel = new Event
                    {
                        EventID = viewModel.EventID,
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        StartTime = viewModel.StartTime,
                        EndTime = viewModel.EndTime,
                        Location = viewModel.Location,
                        CategoryID = viewModel.CategoryID
                    };

                    await _eventService.UpdateEventAsync(eventModel);

                    // SignalR notification
                    await _hubContext.Clients.Group($"Event_{id}").SendAsync("EventUpdated", "Event details have been updated");

                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _eventService.GetEventByIdAsync(viewModel.EventID);
                    if (exists == null)
                        return NotFound();
                    else
                        throw;
                }
            }

            viewModel.Categories = await _context.EventCategories.ToListAsync();
            return View(viewModel);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var eventModel = await _eventService.GetEventByIdAsync(id.Value);
            if (eventModel == null) return NotFound();

            var viewModel = new EventViewModel
            {
                EventID = eventModel.EventID,
                Title = eventModel.Title,
                Description = eventModel.Description,
                StartTime = eventModel.StartTime,
                EndTime = eventModel.EndTime,
                Location = eventModel.Location,
                CategoryName = eventModel.Category?.CategoryName,
                AttendeeCount = await _eventService.GetAttendeeCountAsync(eventModel.EventID)
            };

            return View(viewModel);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventModel = await _eventService.GetEventByIdAsync(id);
            if (eventModel != null)
            {
                await _eventService.DeleteEventAsync(id);

                // SignalR notification
                await _hubContext.Clients.All.SendAsync("EventDeleted", eventModel.Title);

                TempData["Success"] = "Event deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Upcoming
        public async Task<IActionResult> Upcoming()
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            return View(events);
        }

        // GET: Events/Search
        public async Task<IActionResult> Search()
        {
            var searchModel = new EventSearchViewModel
            {
                Categories = await _context.EventCategories.ToListAsync(),
                Events = new List<Event>()
            };
            return View(searchModel);
        }

        // POST: Events/Search
        [HttpPost]
        public async Task<IActionResult> Search(EventSearchViewModel searchModel)
        {
            searchModel.Events = await _eventService.SearchEventsAsync(searchModel);
            searchModel.Categories = await _context.EventCategories.ToListAsync();
            return View(searchModel);
        }
    }
} 