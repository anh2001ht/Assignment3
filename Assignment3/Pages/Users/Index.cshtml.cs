using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Assignment3.Models;
using Assignment3.Data;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UserViewModel> Users { get; set; } = new List<UserViewModel>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string RoleFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ActivityFilter { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // Start with all users
            var query = _context.Users
                .Include(u => u.Attendees)
                .ThenInclude(a => a.Event)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(u => 
                    u.FullName.Contains(SearchTerm) ||
                    u.Username.Contains(SearchTerm) ||
                    (u.Email != null && u.Email.Contains(SearchTerm)));
            }

            // Apply role filter
            if (!string.IsNullOrEmpty(RoleFilter))
            {
                query = query.Where(u => u.Role == RoleFilter);
            }

            // Apply activity filter
            if (!string.IsNullOrEmpty(ActivityFilter))
            {
                if (ActivityFilter == "active")
                {
                    query = query.Where(u => u.Attendees.Any());
                }
                else if (ActivityFilter == "inactive")
                {
                    query = query.Where(u => !u.Attendees.Any());
                }
            }

            // Convert to ViewModels
            Users = await query
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
        }
    }
} 