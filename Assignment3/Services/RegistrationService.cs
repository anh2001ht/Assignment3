using Assignment3.Data;
using Assignment3.Models;
using Assignment3.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public RegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterForEventAsync(int eventId, int userId, string name, string email)
        {
            try
            {
                // Check if already registered
                var existingRegistration = await _context.Attendees
                    .FirstOrDefaultAsync(a => a.EventID == eventId && a.UserID == userId);

                if (existingRegistration != null)
                    return false;

                // Check if event exists
                var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
                if (!eventExists)
                    return false;

                var attendee = new Attendee
                {
                    EventID = eventId,
                    UserID = userId,
                    Name = name,
                    Email = email,
                    RegistrationTime = DateTime.Now
                };

                _context.Attendees.Add(attendee);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnregisterFromEventAsync(int eventId, int userId)
        {
            try
            {
                var attendee = await _context.Attendees
                    .FirstOrDefaultAsync(a => a.EventID == eventId && a.UserID == userId);

                if (attendee == null)
                    return false;

                _context.Attendees.Remove(attendee);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserRegisteredAsync(int eventId, int userId)
        {
            return await _context.Attendees
                .AnyAsync(a => a.EventID == eventId && a.UserID == userId);
        }

        public async Task<List<AttendeeViewModel>> GetEventAttendeesAsync(int eventId)
        {
            return await _context.Attendees
                .Where(a => a.EventID == eventId)
                .Include(a => a.Event)
                .Include(a => a.User)
                .Select(a => new AttendeeViewModel
                {
                    AttendeeID = a.AttendeeID,
                    EventID = a.EventID ?? 1,
                    UserID = a.UserID,
                    Name = a.Name,
                    Email = a.Email,
                    RegistrationTime = a.RegistrationTime,
                    EventTitle = a.Event.Title,
                    UserName = a.User.UserName
                })
                .OrderBy(a => a.RegistrationTime)
                .ToListAsync();
        }

        public async Task<List<EventViewModel>> GetUserRegistrationsAsync(int userId)
        {
            return await _context.Attendees
                .Where(a => a.UserID == userId)
                .Include(a => a.Event)
                .ThenInclude(e => e.Category)
                .Select(a => new EventViewModel
                {
                    EventID = a.Event.EventID,
                    Title = a.Event.Title,
                    Description = a.Event.Description,
                    StartTime = a.Event.StartTime,
                    EndTime = a.Event.EndTime,
                    Location = a.Event.Location,
                    CategoryID = a.Event.CategoryID,
                    CategoryName = a.Event.Category.CategoryName,
                    AttendeeCount = a.Event.Attendees.Count,
                    IsUserRegistered = true
                })
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<int> GetAttendeeCountAsync(int eventId)
        {
            return await _context.Attendees
                .CountAsync(a => a.EventID == eventId);
        }

        public async Task<bool> CanRegisterAsync(int eventId, int userId)
        {
            // Check if event exists and is in the future
            var eventDetails = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventDetails == null || eventDetails.StartTime <= DateTime.Now)
                return false;

            // Check if already registered
            var isRegistered = await IsUserRegisteredAsync(eventId, userId);
            if (isRegistered)
                return false;

            // Check for conflicting events
            var hasConflict = await HasConflictingRegistrationAsync(userId, eventDetails.StartTime ?? DateTime.MinValue, eventDetails.EndTime?? DateTime.MaxValue);
            return !hasConflict;
        }

        public async Task<Attendee> GetAttendeeAsync(int eventId, int userId)
        {
            return await _context.Attendees
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.EventID == eventId && a.UserID == userId);
        }

        public async Task<bool> UpdateAttendeeInfoAsync(int attendeeId, string name, string email)
        {
            try
            {
                var attendee = await _context.Attendees.FindAsync(attendeeId);
                if (attendee == null)
                    return false;

                attendee.Name = name;
                attendee.Email = email;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<AttendeeViewModel>> GetAllAttendeesAsync()
        {
            return await _context.Attendees
                .Include(a => a.Event)
                .Include(a => a.User)
                .Select(a => new AttendeeViewModel
                {
                    AttendeeID = a.AttendeeID,
                    EventID = a.EventID ?? 1,
                    UserID = a.UserID,
                    Name = a.Name,
                    Email = a.Email,
                    RegistrationTime = a.RegistrationTime,
                    EventTitle = a.Event.Title,
                    UserName = a.User.UserName
                })
                .OrderBy(a => a.RegistrationTime)
                .ToListAsync();
        }

        public async Task<bool> HasConflictingRegistrationAsync(int userId, DateTime startTime, DateTime endTime)
        {
            return await _context.Attendees
                .Include(a => a.Event)
                .Where(a => a.UserID == userId)
                .AnyAsync(a => a.Event.StartTime < endTime && a.Event.EndTime > startTime);
        }
    }
} 