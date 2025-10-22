using CMCS.Data;
using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Controllers
{
    public class LecturerController : Controller
    {
        private readonly CMCSContext _context;

        public LecturerController(CMCSContext context)
        {
            _context = context;
        }

        // ===========================
        // Lecturer Dashboard
        // ===========================
        public async Task<IActionResult> Dashboard()
        {
            // Fetch summary info for lecturer view
            var totalClaims = await _context.Claims.CountAsync();
            var pending = await _context.Claims.CountAsync(c => c.Status == "Pending");
            var approved = await _context.Claims.CountAsync(c => c.Status == "Approved");
            var rejected = await _context.Claims.CountAsync(c => c.Status == "Rejected");

            ViewBag.TotalClaims = totalClaims;
            ViewBag.PendingClaims = pending;
            ViewBag.ApprovedClaims = approved;
            ViewBag.RejectedClaims = rejected;

            return View();
        }

        // ===========================
        // Submit Claim (GET)
        // ===========================
        [HttpGet]
        public IActionResult SubmitClaim()
        {
            return View();
        }

        // ===========================
        // Submit Claim (POST)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please ensure all required fields are correctly filled.";
                return View(claim);
            }

            try
            {
                
                claim.LecturerID = 1; 
                claim.ClaimDate = DateTime.Now;
                claim.Status = "Pending";

                
                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                // Handle optional document upload
                if (file != null && file.Length > 0)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    var filePath = Path.Combine(uploadDir, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var document = new Document
                    {
                        ClaimID = claim.ClaimID,
                        FileName = file.FileName,
                        FileType = Path.GetExtension(file.FileName),
                        FilePath = "/uploads/" + file.FileName,
                        UploadDate = DateTime.Now
                    };

                    _context.Documents.Add(document);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "✅ Claim submitted successfully and is awaiting coordinator approval.";
                return RedirectToAction(nameof(TrackStatus));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View(claim);
            }
        }


        // ===========================
        // Track Status (UPDATED VERSION)
        // ===========================
        public async Task<IActionResult> TrackStatus()
        {
            try
            {
                var claims = await _context.Claims
                    .OrderByDescending(c => c.ClaimDate)
                    .ToListAsync();

                return View(claims);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred while loading your claims: {ex.Message}";
                return View(new List<Claim>());
            }
        }

        // ===========================
        // Upload Document (GET)
        // ===========================
        [HttpGet]
        public IActionResult UploadDocument()
        {
            ViewBag.Claims = _context.Claims.ToList();
            return View();
        }

        // ===========================
        // Upload Document (POST)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int claimId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.ErrorMessage = "Please select a valid file to upload.";
                ViewBag.Claims = _context.Claims.ToList();
                return View();
            }

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var filePath = Path.Combine(uploadDir, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                ClaimID = claimId,
                FileName = file.FileName,
                FileType = Path.GetExtension(file.FileName),
                FilePath = "/uploads/" + file.FileName,
                UploadDate = DateTime.Now
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Document successfully uploaded.";
            ViewBag.Claims = _context.Claims.ToList();
            return View();
        }
    }
}