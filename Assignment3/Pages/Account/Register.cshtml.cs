using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.Services;
using Assignment3.ViewModels;
using Assignment3.Models;

namespace Assignment3.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;

        public RegisterModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                if (await _userService.UserExistsAsync(Input.Username))
                {
                    ModelState.AddModelError("Input.Username", "Username already exists");
                    return Page();
                }

                var user = new User
                {
                    Username = Input.Username,
                    FullName = Input.FullName,
                    Email = Input.Email,
                    Role = Input.Role
                };

                try
                {
                    await _userService.RegisterAsync(user, Input.Password);
                    TempData["Success"] = "Registration successful! Please login.";
                    return RedirectToPage("/Account/Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Registration failed: " + ex.Message);
                }
            }
            return Page();
        }
    }
} 