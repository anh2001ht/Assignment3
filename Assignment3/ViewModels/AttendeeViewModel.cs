using System.ComponentModel.DataAnnotations;
using Assignment3.Models;

namespace Assignment3.ViewModels
{
    public class AttendeeViewModel
    {
        public int AttendeeID { get; set; }
        public int EventID { get; set; }
        public int? UserID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        public DateTime? RegistrationTime { get; set; }
        
        // Navigation properties for display
        public string? EventTitle { get; set; }
        public string? UserName { get; set; }
    }

    public class EventRegistrationViewModel
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Location { get; set; }
        public string? CategoryName { get; set; }
        public int AttendeeCount { get; set; }
        public bool IsUserRegistered { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;
    }
} 