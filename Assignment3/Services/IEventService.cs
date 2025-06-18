using Assignment3.Models;
using Assignment3.ViewModels;

namespace Assignment3.Services
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event?> GetEventByIdAsync(int id);
        Task<Event> CreateEventAsync(Event eventModel);
        Task<Event> UpdateEventAsync(Event eventModel);
        Task DeleteEventAsync(int id);
        Task<IEnumerable<Event>> SearchEventsAsync(EventSearchViewModel searchModel);
        Task<IEnumerable<Event>> GetEventsByCategoryAsync(int categoryId);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetAttendeeCountAsync(int eventId);
        Task<bool> IsUserRegisteredAsync(int eventId, int userId);
    }
} 