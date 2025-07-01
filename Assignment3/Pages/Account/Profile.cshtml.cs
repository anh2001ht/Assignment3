using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.Services;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using System.Security.Claims;

namespace Assignment3.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public ProfileModel(IUserService userService, ApplicationDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [BindProperty]
        public ProfileEditViewModel Input { get; set; } = new();

        public UserProfileViewModel UserProfile { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _context.Users
                .Include(u => u.Attendees)
                    .ThenInclude(a => a.Event)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var now = DateTime.Now;
            var registeredEvents = user.Attendees.Where(a => a.Event != null).Select(a => a.Event!).ToList();

            UserProfile = new UserProfileViewModel
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                TotalRegistrations = registeredEvents.Count,
                UpcomingEvents = registeredEvents.Count(e => e.StartTime > now),
                MemberSince = user.Attendees.Any(a => a.RegistrationTime.HasValue) 
                    ? user.Attendees.Where(a => a.RegistrationTime.HasValue).Min(a => a.RegistrationTime.Value)
                    : DateTime.Now,
                RecentEvents = registeredEvents
                    .OrderByDescending(e => e.StartTime)
                    .Select(e => new EventSummaryViewModel
                    {
                        EventID = e.EventID,
                        Title = e.Title,
                        StartTime = e.StartTime ?? DateTime.Now,
                        CategoryName = e.Category?.CategoryName ?? "Uncategorized"
                    }).ToList()
            };

            Input = new ProfileEditViewModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload profile data
                return Page();
            }

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                // Check if username is taken by another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == Input.Username && u.UserID != user.UserID);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Input.Username", "Username is already taken.");
                    await OnGetAsync(); // Reload profile data
                    return Page();
                }

                // Update user information
                user.Username = Input.Username;
                user.FullName = Input.FullName;
                user.Email = Input.Email;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                return RedirectToPage();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating your profile. Please try again.");
                await OnGetAsync(); // Reload profile data
                return Page();
            }
        }
    }
} 