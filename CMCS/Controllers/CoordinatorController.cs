using CMCS.Data;
using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly CMCSContext _context;
        private readonly ILogger<CoordinatorController> _logger;

        public CoordinatorController(CMCSContext context, ILogger<CoordinatorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ------------------ DASHBOARD ------------------
        public IActionResult Dashboard()
        {
            try
            {
                ViewBag.TotalClaims = _context.Claims.Count();
                ViewBag.PendingClaims = _context.Claims.Count(c => c.Status == "Pending");
                ViewBag.ApprovedClaims = _context.Claims.Count(c => c.Status == "Approved");
                ViewBag.RejectedClaims = _context.Claims.Count(c => c.Status == "Rejected");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Coordinator Dashboard.");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ------------------ APPROVALS LIST ------------------
        public IActionResult Approvals()
        {
            try
            {
                // Join Claims with Lecturers for display (avoid .Include() on scalar property)
                var claims = _context.Claims
                    .Join(
                        _context.Lecturers,
                        claim => claim.LecturerID,
                        lecturer => lecturer.LecturerID,
                        (claim, lecturer) => new
                        {
                            claim.ClaimID,
                            claim.ClaimDate,
                            claim.HoursWorked,
                            claim.HourlyRate,
                            claim.Status,
                            LecturerName = lecturer.FirstName + " " + lecturer.LastName
                        }
                    )
                    .OrderByDescending(c => c.ClaimDate)
                    .ToList();

                if (!claims.Any())
                    TempData["Info"] = "No claims found in the system.";

                return View(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Approvals view.");
                TempData["Error"] = "Unable to load claims at the moment.";
                return RedirectToAction("Dashboard");
            }
        }

        // ------------------ VIEW CLAIM DETAILS ------------------
        public IActionResult ViewClaimDetails(int id)
        {
            try
            {
                var claim = _context.Claims.FirstOrDefault(c => c.ClaimID == id);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction("Approvals");
                }

                ViewBag.Lecturer = _context.Lecturers.FirstOrDefault(l => l.LecturerID == claim.LecturerID);
                ViewBag.Documents = _context.Documents.Where(d => d.ClaimID == id).ToList();

                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claim details for Claim ID {ClaimID}", id);
                TempData["Error"] = "Unable to load claim details.";
                return RedirectToAction("Approvals");
            }
        }

        // ------------------ UPDATE CLAIM STATUS ------------------
        [HttpPost]
        public IActionResult UpdateClaimStatus(int claimId, string status, string comments)
        {
            try
            {
                var claim = _context.Claims.FirstOrDefault(c => c.ClaimID == claimId);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction("Approvals");
                }

                if (string.IsNullOrWhiteSpace(comments))
                {
                    TempData["Error"] = "Please provide comments before updating the claim status.";
                    return RedirectToAction("ViewClaimDetails", new { id = claimId });
                }

                claim.Status = status;
                _context.SaveChanges();

                // Log to AuditTrail
                _context.AuditTrails.Add(new AuditTrail
                {
                    ClaimID = claimId,
                    RoleID = 2, // Coordinator role
                    ActionType = status,
                    ActionDate = DateTime.Now,
                    Comments = comments
                });

                // Send notification to lecturer
                var lecturer = _context.Lecturers.FirstOrDefault(l => l.LecturerID == claim.LecturerID);
                if (lecturer != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        ClaimID = claimId,
                        LecturerID = lecturer.LecturerID,
                        Message = $"Your claim (ID: {claimId}) was {status.ToLower()} by the coordinator.",
                        DateSent = DateTime.Now,
                        Status = "Unread"
                    });
                }

                _context.SaveChanges();
                TempData["Success"] = $"Claim successfully {status.ToLower()}!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating claim status for Claim ID {ClaimID}", claimId);
                TempData["Error"] = "An unexpected error occurred while updating claim status.";
            }

            return RedirectToAction("Approvals");
        }
    }
}
