using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment3.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        public int? CategoryID { get; set; }

        // Navigation properties
        [ForeignKey("CategoryID")]
        public virtual EventCategory? Category { get; set; }

        public virtual ICollection<Attendee> Attendees { get; set; } = new List<Attendee>();
    }
} 