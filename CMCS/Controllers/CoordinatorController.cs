using Microsoft.AspNetCore.Mvc;

namespace CMCS.Controllers
{
    public class CoordinatorController : Controller
    {
        // Coordinator dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // Approvals screen
        public IActionResult Approvals()
        {
            return View();
        }
    }
}
