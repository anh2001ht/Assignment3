using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.Data;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string FilterType { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Start with all categories and include events count
            var query = _context.EventCategories
                .Include(c => c.Events)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(c => c.CategoryName.Contains(SearchTerm));
            }

            // Convert to ViewModels
            var categoriesData = await query
                .Select(c => new CategoryViewModel
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    EventCount = c.Events.Count,
                    CreatedDate = DateTime.Now // Note: EventCategory doesn't have CreatedDate, using current date as placeholder
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            // Apply filter after data retrieval
            if (!string.IsNullOrEmpty(FilterType))
            {
                if (FilterType == "active")
                {
                    categoriesData = categoriesData.Where(c => c.EventCount > 0).ToList();
                }
                else if (FilterType == "inactive")
                {
                    categoriesData = categoriesData.Where(c => c.EventCount == 0).ToList();
                }
            }

            Categories = categoriesData;
        }
    }
} 