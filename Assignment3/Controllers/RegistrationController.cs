using Assignment3.Services;
using Assignment3.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Assignment3.Hubs;
using System.Security.Claims;

namespace Assignment3.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationService _registrationService;
        private readonly IEventService _eventService;
        private readonly IHubContext<EventHub> _hubContext;

        public RegistrationController(
            IRegistrationService registrationService,
            IEventService eventService,
            IHubContext<EventHub> hubContext)
        {
            _registrationService = registrationService;
            _eventService = eventService;
            _hubContext = hubContext;
        }

        // GET: Registration/Register/5
        public async Task<IActionResult> Register(int id)
        {
            var eventModel = await _eventService.GetEventByIdAsync(id);
            if (eventModel == null)
                return NotFound();

            var userId = GetCurrentUserId();
            var canRegister = await _registrationService.CanRegisterAsync(id, userId);

            if (!canRegister)
            {
                TempData["Error"] = "You cannot register for this event. It may have already started, you may already be registered, or you have a conflicting event.";
                return RedirectToAction("Details", "Events", new { id = id });
            }

            var attendeeCount = await _registrationService.GetAttendeeCountAsync(id);

            var viewModel = new EventRegistrationViewModel
            {
                EventID = eventModel.EventID,
                EventTitle = eventModel.Title,
                EventDescription = eventModel.Description,
                StartTime = eventModel.StartTime,
                EndTime = eventModel.EndTime,
                Location = eventModel.Location,
                //CategoryName = eventModel.CategoryName,
                AttendeeCount = attendeeCount,
                IsUserRegistered = false
            };

            return View(viewModel);
        }

        // POST: Registration/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(EventRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                var canRegister = await _registrationService.CanRegisterAsync(model.EventID, userId);

                if (!canRegister)
                {
                    TempData["Error"] = "Registration failed. Please try again.";
                    return View(model);
                }

                var success = await _registrationService.RegisterForEventAsync(
                    model.EventID, userId, model.Name, model.Email);

                if (success)
                {
                    // Send SignalR notification
                    await _hubContext.Clients.All.SendAsync("EventRegistrationUpdate", new 
                    {
                        EventId = model.EventID,
                        Message = $"New registration for {model.EventTitle}",
                        AttendeeCount = await _registrationService.GetAttendeeCountAsync(model.EventID)
                    });

                    TempData["Success"] = "Successfully registered for the event!";
                    return RedirectToAction("Details", "Events", new { id = model.EventID });
                }
                else
                {
                    TempData["Error"] = "Registration failed. Please try again.";
                }
            }

            return View(model);
        }

        // POST: Registration/Unregister/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unregister(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _registrationService.UnregisterFromEventAsync(id, userId);

            if (success)
            {
                // Send SignalR notification
                await _hubContext.Clients.All.SendAsync("EventRegistrationUpdate", new 
                {
                    EventId = id,
                    Message = "User unregistered from event",
                    AttendeeCount = await _registrationService.GetAttendeeCountAsync(id)
                });

                TempData["Success"] = "Successfully unregistered from the event.";
            }
            else
            {
                TempData["Error"] = "Failed to unregister. Please try again.";
            }

            return RedirectToAction("Details", "Events", new { id = id });
        }

        // GET: Registration/MyRegistrations
        public async Task<IActionResult> MyRegistrations()
        {
            var userId = GetCurrentUserId();
            var registrations = await _registrationService.GetUserRegistrationsAsync(userId);
            return View(registrations);
        }

        // GET: Registration/Attendees/5
        public async Task<IActionResult> Attendees(int id)
        {
            var eventModel = await _eventService.GetEventByIdAsync(id);
            if (eventModel == null)
                return NotFound();

            var attendees = await _registrationService.GetEventAttendeesAsync(id);
            ViewBag.EventTitle = eventModel.Title;
            ViewBag.EventID = id;
            
            return View(attendees);
        }

        // GET: Registration/AllAttendees
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllAttendees()
        {
            var attendees = await _registrationService.GetAllAttendeesAsync();
            return View(attendees);
        }

        // GET: Registration/UpdateInfo/5
        public async Task<IActionResult> UpdateInfo(int eventId)
        {
            var userId = GetCurrentUserId();
            var attendee = await _registrationService.GetAttendeeAsync(eventId, userId);
            
            if (attendee == null)
                return NotFound();

            var viewModel = new AttendeeViewModel
            {
                AttendeeID = attendee.AttendeeID,
                EventID = attendee.EventID ?? 1,
                UserID = attendee.UserID,
                Name = attendee.Name,
                Email = attendee.Email,
                RegistrationTime = attendee.RegistrationTime,
                EventTitle = attendee.Event?.Title
            };

            return View(viewModel);
        }

        // POST: Registration/UpdateInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(AttendeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _registrationService.UpdateAttendeeInfoAsync(
                    model.AttendeeID, model.Name, model.Email);

                if (success)
                {
                    TempData["Success"] = "Your registration information has been updated.";
                    return RedirectToAction("Details", "Events", new { id = model.EventID });
                }
                else
                {
                    TempData["Error"] = "Failed to update information. Please try again.";
                }
            }

            return View(model);
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
} 