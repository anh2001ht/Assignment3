using Assignment3.Data;
using Assignment3.Models;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;

        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.EventID == id);
        }

        public async Task<Event> CreateEventAsync(Event eventModel)
        {
            _context.Events.Add(eventModel);
            await _context.SaveChangesAsync();
            return eventModel;
        }

        public async Task<Event> UpdateEventAsync(Event eventModel)
        {
            _context.Events.Update(eventModel);
            await _context.SaveChangesAsync();
            return eventModel;
        }

        public async Task DeleteEventAsync(int id)
        {
            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete != null)
            {
                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(EventSearchViewModel searchModel)
        {
            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                query = query.Where(e => e.Title.Contains(searchModel.SearchTerm) ||
                                        e.Description!.Contains(searchModel.SearchTerm) ||
                                        e.Location!.Contains(searchModel.SearchTerm));
            }

            if (searchModel.CategoryId.HasValue)
            {
                query = query.Where(e => e.CategoryID == searchModel.CategoryId.Value);
            }

            if (searchModel.StartDate.HasValue)
            {
                query = query.Where(e => e.StartTime >= searchModel.StartDate.Value);
            }

            if (searchModel.EndDate.HasValue)
            {
                query = query.Where(e => e.EndTime <= searchModel.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(searchModel.Location))
            {
                query = query.Where(e => e.Location!.Contains(searchModel.Location));
            }

            return await query.OrderByDescending(e => e.StartTime).ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByCategoryAsync(int categoryId)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .Where(e => e.CategoryID == categoryId)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            var currentDate = DateTime.Now;
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .Where(e => e.StartTime >= currentDate)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Attendees)
                .Where(e => e.StartTime >= startDate && e.EndTime <= endDate)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<int> GetAttendeeCountAsync(int eventId)
        {
            return await _context.Attendees
                .CountAsync(a => a.EventID == eventId);
        }

        public async Task<bool> IsUserRegisteredAsync(int eventId, int userId)
        {
            return await _context.Attendees
                .AnyAsync(a => a.EventID == eventId && a.UserID == userId);
        }
    }
} 