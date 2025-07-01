using System.ComponentModel.DataAnnotations;

namespace Assignment3.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateCategoryViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class EditCategoryViewModel
    {
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class DeleteCategoryViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int EventCount { get; set; }
        public bool CanDelete => EventCount == 0;
    }
} 