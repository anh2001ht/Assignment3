using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;

namespace Assignment3.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public DeleteUserViewModel UserToDelete { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Attendees)
                    .ThenInclude(a => a.Event)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage("./Index");
            }

            var now = DateTime.Now;
            var activeAttendees = user.Attendees.Count(a => a.Event != null && a.Event.StartTime > now);

            UserToDelete = new DeleteUserViewModel
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                TotalRegistrations = user.Attendees.Count,
                HasActiveRegistrations = activeAttendees > 0
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Attendees)
                    .ThenInclude(a => a.Event)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage("./Index");
            }

            // Check for active registrations
            var now = DateTime.Now;
            var activeAttendees = user.Attendees.Where(a => a.Event != null && a.Event.StartTime > now).ToList();

            if (activeAttendees.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete user with active event registrations. Please cancel all active registrations first.";
                return RedirectToPage("./Details", new { id = user.UserID });
            }

            try
            {
                // Remove all attendee records first
                _context.Attendees.RemoveRange(user.Attendees);
                
                // Remove the user
                _context.Users.Remove(user);
                
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User '{user.FullName}' has been deleted successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the user. Please try again.";
                return RedirectToPage("./Details", new { id = user.UserID });
            }
        }
    }
}