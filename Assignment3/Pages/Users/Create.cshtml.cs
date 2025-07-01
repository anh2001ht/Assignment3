using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Assignment3.Models;
using Assignment3.Services;
using Assignment3.ViewModels;

namespace Assignment3.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IUserService _userService;

        public CreateModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public CreateUserViewModel Input { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _userService.UserExistsAsync(Input.Username))
                {
                    ModelState.AddModelError("Input.Username", "Username already exists.");
                    return Page();
                }

                var user = new User
                {
                    Username = Input.Username,
                    FullName = Input.FullName,
                    Email = Input.Email,
                    Role = Input.Role ?? "User"
                };

                try
                {
                    await _userService.RegisterAsync(user, Input.Password);
                    TempData["Success"] = $"User '{user.FullName}' has been created successfully.";
                    return RedirectToPage("./Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating user: " + ex.Message);
                }
            }

            return Page();
        }
    }
} 