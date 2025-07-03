using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using Assignment3.Models;
using Assignment3.Services;
using Assignment3.ViewModels;

namespace Assignment3.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public UsersController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Attendees)
                .ThenInclude(a => a.Event)
                .Select(u => new UserViewModel
                {
                    UserID = u.UserID,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    TotalRegistrations = u.Attendees.Count,
                    LastActivity = u.Attendees.OrderByDescending(a => a.RegistrationTime).FirstOrDefault().RegistrationTime,
                    UpcomingEvents = u.Attendees.Count(a => a.Event.StartTime.HasValue && a.Event.StartTime.Value > DateTime.Now)
                })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Attendees)
                .ThenInclude(a => a.Event)
                .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(m => m.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            var userDetails = new UserDetailsViewModel
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                TotalRegistrations = user.Attendees.Count,
                LastActivity = user.Attendees.OrderByDescending(a => a.RegistrationTime).FirstOrDefault()?.RegistrationTime,
                UpcomingEvents = user.Attendees.Count(a => a.Event.StartTime.HasValue && a.Event.StartTime.Value > DateTime.Now),
                PastEvents = user.Attendees.Count(a => a.Event.EndTime.HasValue && a.Event.EndTime.Value < DateTime.Now),
                RegisteredEvents = user.Attendees.Select(a => new EventSummaryViewModel
                {
                    EventID = a.Event.EventID,
                    Title = a.Event.Title,
                    StartTime = a.Event.StartTime ?? DateTime.MinValue,
                    CategoryName = a.Event.Category?.CategoryName ?? "No Category",
                    AttendeeCount = a.Event.Attendees.Count
                }).OrderByDescending(e => e.StartTime).ToList()
            };

            return View(userDetails);
        }

        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Username,Password,FullName,Email,Role")] CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _userService.UserExistsAsync(model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = model.Role
                };

                try
                {
                    await _userService.RegisterAsync(user, model.Password);
                    TempData["SuccessMessage"] = $"User '{user.FullName}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating user: " + ex.Message);
                }
            }
            return View(model);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var editModel = new EditUserViewModel
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return View(editModel);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("UserID,Username,FullName,Email,Role")] EditUserViewModel model)
        {
            if (id != model.UserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Check if username is being changed and if new username already exists
                    if (user.Username != model.Username && await _userService.UserExistsAsync(model.Username))
                    {
                        ModelState.AddModelError("Username", "Username already exists.");
                        return View(model);
                    }

                    user.Username = model.Username;
                    user.FullName = model.FullName;
                    user.Email = model.Email;
                    user.Role = model.Role;

                    await _userService.UpdateAsync(user);
                    TempData["SuccessMessage"] = $"User '{user.FullName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExists(model.UserID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating user: " + ex.Message);
                }
            }
            return View(model);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Attendees)
                .FirstOrDefaultAsync(m => m.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            var deleteModel = new DeleteUserViewModel
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                TotalRegistrations = user.Attendees.Count,
                HasActiveRegistrations = user.Attendees.Any(a => a.Event.StartTime.HasValue && a.Event.StartTime.Value > DateTime.Now)
            };

            return View(deleteModel);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Attendees)
                    .FirstOrDefaultAsync(u => u.UserID == id);

                if (user == null)
                {
                    return NotFound();
                }

                // Check if user has active registrations
                var hasActiveRegistrations = user.Attendees.Any(a => a.Event.StartTime.HasValue && a.Event.StartTime.Value > DateTime.Now);
                if (hasActiveRegistrations)
                {
                    TempData["ErrorMessage"] = "Cannot delete user with active event registrations.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                await _userService.DeleteAsync(id);
                TempData["SuccessMessage"] = $"User '{user.FullName}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting user: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        // GET: Users/Profile
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _context.Users
                .Include(u => u.Attendees)
                .ThenInclude(a => a.Event)
                .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

            if (currentUser == null)
            {
                return NotFound();
            }

            var userProfile = new UserProfileViewModel
            {
                UserID = currentUser.UserID,
                Username = currentUser.Username,
                FullName = currentUser.FullName,
                Email = currentUser.Email,
                Role = currentUser.Role,
                TotalRegistrations = currentUser.Attendees.Count,
                UpcomingEvents = currentUser.Attendees.Count(a => a.Event.StartTime.HasValue && a.Event.StartTime.Value > DateTime.Now),
                PastEvents = currentUser.Attendees.Count(a => a.Event.EndTime.HasValue && a.Event.EndTime.Value < DateTime.Now),
                RegisteredEvents = currentUser.Attendees.Select(a => new EventSummaryViewModel
                {
                    EventID = a.Event.EventID,
                    Title = a.Event.Title,
                    StartTime = a.Event.StartTime ?? DateTime.MinValue,
                    CategoryName = a.Event.Category?.CategoryName ?? "No Category",
                    AttendeeCount = a.Event.Attendees.Count
                }).OrderByDescending(e => e.StartTime).ToList()
            };

            return View(userProfile);
        }

        private async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserID == id);
        }
    }
}