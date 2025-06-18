using System.ComponentModel.DataAnnotations;

namespace Assignment3.ViewModels
{
    public class UserViewModel
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int TotalRegistrations { get; set; }
        public DateTime? LastActivity { get; set; }
        public int UpcomingEvents { get; set; }
    }

    public class UserDetailsViewModel
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int TotalRegistrations { get; set; }
        public DateTime? LastActivity { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
        public List<EventSummaryViewModel> RegisteredEvents { get; set; } = new List<EventSummaryViewModel>();
    }

    public class CreateUserViewModel
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Role")]
        public string? Role { get; set; }
    }

    public class EditUserViewModel
    {
        public int UserID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Role")]
        public string? Role { get; set; }
    }

    public class DeleteUserViewModel
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int TotalRegistrations { get; set; }
        public bool HasActiveRegistrations { get; set; }
    }

    public class UserProfileViewModel
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int TotalRegistrations { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
        public List<EventSummaryViewModel> RegisteredEvents { get; set; } = new List<EventSummaryViewModel>();
    }
} 