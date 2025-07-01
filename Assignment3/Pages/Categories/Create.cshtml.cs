using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.Data;
using Assignment3.ViewModels;
using Assignment3.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateCategoryViewModel CreateCategory { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if category name already exists
            var existingCategory = await _context.EventCategories
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == CreateCategory.CategoryName.ToLower());

            if (existingCategory != null)
            {
                ModelState.AddModelError("CreateCategory.CategoryName", 
                    "A category with this name already exists.");
                return Page();
            }

            try
            {
                var category = new EventCategory
                {
                    CategoryName = CreateCategory.CategoryName.Trim()
                };

                _context.EventCategories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Category '{category.CategoryName}' has been created successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred while creating the category. Please try again.";
                return Page();
            }
        }
    }
} 