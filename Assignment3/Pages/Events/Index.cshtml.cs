using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Assignment3.Models;
using Assignment3.Services;
using Assignment3.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.Pages.Events
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;

        public IndexModel(IEventService eventService, ApplicationDbContext context)
        {
            _eventService = eventService;
            _context = context;
        }

        public IList<Event> Events { get; set; } = new List<Event>();
        public IList<EventCategory> Categories { get; set; } = new List<EventCategory>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Location { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.EventCategories.ToListAsync();

            // Apply search filters
            if (!string.IsNullOrEmpty(SearchTerm) || CategoryId.HasValue || 
                StartDate.HasValue || EndDate.HasValue || 
                !string.IsNullOrEmpty(Location))
            {
                var query = _context.Events
                    .Include(e => e.Category)
                    .Include(e => e.Attendees)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(e => e.Title.Contains(SearchTerm) || 
                                           e.Description!.Contains(SearchTerm) || 
                                           e.Location!.Contains(SearchTerm));
                }

                if (CategoryId.HasValue)
                {
                    query = query.Where(e => e.CategoryID == CategoryId);
                }

                if (StartDate.HasValue)
                {
                    query = query.Where(e => e.StartTime >= StartDate);
                }

                if (EndDate.HasValue)
                {
                    query = query.Where(e => e.EndTime <= EndDate);
                }

                if (!string.IsNullOrEmpty(Location))
                {
                    query = query.Where(e => e.Location!.Contains(Location));
                }

                Events = await query.OrderBy(e => e.StartTime).ToListAsync();
            }
            else
            {
                var allEvents = await _eventService.GetAllEventsAsync();
                Events = allEvents.ToList();
            }
        }
    }
} 