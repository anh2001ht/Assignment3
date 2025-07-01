using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Assignment3.Models;
using Assignment3.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment3.Pages.Attendees
{
    [Authorize]
    public class MyRegistrationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MyRegistrationsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Attendee> Registrations { get; set; } = new List<Attendee>();

        public async Task OnGetAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            Registrations = await _context.Attendees
                .Include(a => a.Event)
                .ThenInclude(e => e!.Category)
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.RegistrationTime)
                .ToListAsync();
        }
    }
} 