namespace Assignment3.ViewModels
{
    public class ReportDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalAttendees { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
        public int EventsThisMonth { get; set; }
        public int RegistrationsThisMonth { get; set; }
        public double AverageAttendeesPerEvent { get; set; }
        public List<CategoryStatViewModel> CategoryStats { get; set; } = new List<CategoryStatViewModel>();
        public List<EventSummaryViewModel> RecentEvents { get; set; } = new List<EventSummaryViewModel>();
        public List<MonthlyStatViewModel> MonthlyStats { get; set; } = new List<MonthlyStatViewModel>();
    }

    public class CategoryStatViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public int TotalAttendees { get; set; }
    }

    public class EventSummaryViewModel
    {
        public int EventID { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int AttendeeCount { get; set; }
    }

    public class MonthlyStatViewModel
    {
        public string Month { get; set; } = string.Empty;
        public int EventsCount { get; set; }
        public int RegistrationsCount { get; set; }
    }

    public class EventAnalyticsViewModel
    {
        public List<EventPerformanceViewModel> EventPerformance { get; set; } = new List<EventPerformanceViewModel>();
        public List<TimeSlotPopularityViewModel> PopularTimeSlots { get; set; } = new List<TimeSlotPopularityViewModel>();
        public List<CategoryAttendanceViewModel> AttendanceByCategory { get; set; } = new List<CategoryAttendanceViewModel>();
    }

    public class EventPerformanceViewModel
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int AttendeeCount { get; set; }
        public double RegistrationRate { get; set; }
    }

    public class TimeSlotPopularityViewModel
    {
        public int Hour { get; set; }
        public int EventCount { get; set; }
        public int TotalAttendees { get; set; }
        public string TimeSlot => $"{Hour:D2}:00 - {(Hour + 1):D2}:00";
    }

    public class CategoryAttendanceViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public int TotalAttendees { get; set; }
        public double AverageAttendeesPerEvent { get; set; }
    }

    public class UserActivityViewModel
    {
        public List<UserActivitySummaryViewModel> ActiveUsers { get; set; } = new List<UserActivitySummaryViewModel>();
        public List<MonthlyStatViewModel> RegistrationTrends { get; set; } = new List<MonthlyStatViewModel>();
    }

    public class UserActivitySummaryViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalRegistrations { get; set; }
        public DateTime? LastRegistration { get; set; }
        public int UpcomingEvents { get; set; }
    }

    // Additional missing view models
    public class EventReportViewModel
    {
        public int EventID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public int AttendeeCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AttendeeReportViewModel
    {
        public string AttendeeName { get; set; } = string.Empty;
        public string AttendeeEmail { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class CategoryReportViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public int TotalAttendees { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
    }
} 