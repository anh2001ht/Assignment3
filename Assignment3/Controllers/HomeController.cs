using Assignment3.Models;
using Assignment3.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Assignment3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Test database connection
            try
            {
                var canConnect = _context.Database.CanConnect();
                ViewBag.StatusClass = canConnect ? "text-success" : "text-danger";
            }
            catch (Exception ex)
            {
                ViewBag.DatabaseStatus = $"Lỗi kết nối: {ex.Message}";
                ViewBag.StatusClass = "text-danger";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
