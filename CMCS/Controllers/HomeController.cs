using System.Diagnostics;
using CMCS.Data;
using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CMCSContext _context;

        public HomeController(ILogger<HomeController> logger, CMCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ===========================
        // Home Page
        // ===========================
        public async Task<IActionResult> Index()
        {
            // Quick summary stats to make the homepage informative
            ViewBag.TotalLecturers = await _context.Lecturers.CountAsync();
            ViewBag.TotalClaims = await _context.Claims.CountAsync();
            ViewBag.PendingClaims = await _context.Claims.CountAsync(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = await _context.Claims.CountAsync(c => c.Status == "Approved");

            return View();
        }

        // ===========================
        // Privacy / Info Page
        // ===========================
        public IActionResult Privacy()
        {
            return View();
        }

        // ===========================
        // Error Handling
        // ===========================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var error = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            _logger.LogError("An unexpected error occurred. RequestId: {RequestId}", error.RequestId);

            return View(error);
        }
    }
}
