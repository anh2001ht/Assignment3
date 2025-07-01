using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment3.Data;

namespace Assignment3.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            // Test database connection
            try
            {
                var canConnect = _context.Database.CanConnect();
                ViewData["StatusClass"] = canConnect ? "text-success" : "text-danger";
            }
            catch (Exception ex)
            {
                ViewData["DatabaseStatus"] = $"Lỗi kết nối: {ex.Message}";
                ViewData["StatusClass"] = "text-danger";
            }
        }
    }
} 