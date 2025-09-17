using Microsoft.AspNetCore.Mvc;

namespace CMCS.Controllers
{
    public class LecturerController : Controller
    {
        // Lecturer dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // claim form
        public IActionResult SubmitClaim()
        {
            return View();
        }

        // Track submitted claims
        public IActionResult TrackStatus()
        {
            return View();
        }

        // Upload supporting documents
        public IActionResult UploadDocument()
        {
            return View();
        }
    }
}
