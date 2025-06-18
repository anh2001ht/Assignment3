using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment3.Models
{
    public class Attendee
    {
        [Key]
        public int AttendeeID { get; set; }

        public int? EventID { get; set; }

        public int? UserID { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        public DateTime? RegistrationTime { get; set; }

        // Navigation properties
        [ForeignKey("EventID")]
        public virtual Event? Event { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
} 