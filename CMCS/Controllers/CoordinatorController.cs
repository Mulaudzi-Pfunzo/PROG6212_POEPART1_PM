using Microsoft.AspNetCore.Mvc;

namespace CMCS.Controllers
{
    public class CoordinatorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
