using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.Services;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;

namespace Assignment3.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public EditModel(IUserService userService, ApplicationDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [BindProperty]
        public EditUserViewModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage("./Index");
            }

            Input = new EditUserViewModel
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == Input.UserID);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToPage("./Index");
                }

                // Check if username is taken by another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == Input.Username && u.UserID != Input.UserID);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Input.Username", "Username is already taken.");
                    return Page();
                }

                // Update user information
                user.Username = Input.Username;
                user.FullName = Input.FullName;
                user.Email = Input.Email;
                user.Role = Input.Role;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User '{Input.FullName}' has been updated successfully.";
                return RedirectToPage("./Details", new { id = Input.UserID });
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the user. Please try again.");
                return Page();
            }
        }
    }
} 