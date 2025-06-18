using Assignment3.Models;
using Assignment3.ViewModels;

namespace Assignment3.Services
{
    public interface IRegistrationService
    {
        Task<bool> RegisterForEventAsync(int eventId, int userId, string name, string email);
        Task<bool> UnregisterFromEventAsync(int eventId, int userId);
        Task<bool> IsUserRegisteredAsync(int eventId, int userId);
        Task<List<AttendeeViewModel>> GetEventAttendeesAsync(int eventId);
        Task<List<EventViewModel>> GetUserRegistrationsAsync(int userId);
        Task<int> GetAttendeeCountAsync(int eventId);
        Task<bool> CanRegisterAsync(int eventId, int userId);
        Task<Attendee> GetAttendeeAsync(int eventId, int userId);
        Task<bool> UpdateAttendeeInfoAsync(int attendeeId, string name, string email);
        Task<List<AttendeeViewModel>> GetAllAttendeesAsync();
        Task<bool> HasConflictingRegistrationAsync(int userId, DateTime startTime, DateTime endTime);
    }
} 