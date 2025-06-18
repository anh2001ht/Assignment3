using System.ComponentModel.DataAnnotations;
using Assignment3.Models;

namespace Assignment3.ViewModels
{
    public class EventViewModel
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public DateTime? StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime? EndTime { get; set; }

        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int? CategoryID { get; set; }

        public string? CategoryName { get; set; }
        public int AttendeeCount { get; set; }
        public bool IsUserRegistered { get; set; }

        public IEnumerable<EventCategory> Categories { get; set; } = new List<EventCategory>();
    }

    public class EventSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        
        public IEnumerable<Event> Events { get; set; } = new List<Event>();
        public IEnumerable<EventCategory> Categories { get; set; } = new List<EventCategory>();
    }
} 