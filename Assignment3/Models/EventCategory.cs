using System.ComponentModel.DataAnnotations;

namespace Assignment3.Models
{
    public class EventCategory
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(255)]
        public string CategoryName { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
} 