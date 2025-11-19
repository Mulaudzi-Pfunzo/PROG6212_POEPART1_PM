using Microsoft.AspNetCore.Mvc;
using CMCS.Data;
using CMCS.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CMCS.Controllers
{
    public class HRController : Controller
    {
        private readonly CMCSContext _context;

        public HRController(CMCSContext context)
        {
            _context = context;
        }

        // 0. DEFAULT INDEX → REDIRECT TO DASHBOARD
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // 1. HR DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.ApprovedCount = await _context.Claims.CountAsync(c => c.Status == "Approved");
            ViewBag.PendingCount = await _context.Claims.CountAsync(c => c.Status == "Pending");
            ViewBag.PaidCount = await _context.HrPaymentRecords.CountAsync();
            ViewBag.TotalLecturers = await _context.Lecturers.CountAsync();

            return View();
        }

        // 2. LIST APPROVED CLAIMS (READY FOR PAYMENT) → FIXED
        public async Task<IActionResult> ApprovedClaims()
        {
            var claims = await _context.Claims
                .Where(c => c.Status == "Approved")
                .Include(c => c.Lecturer) // properly include Lecturer
                .ToListAsync();

            return View(claims);
        }

        // 3. PROCESS PAYMENT (SHOW CLAIM & LECTURER INFO)
        public async Task<IActionResult> ProcessPayment(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer)  // eager-load lecturer
                .FirstOrDefaultAsync(c => c.ClaimID == id);

            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("ApprovedClaims");
            }

            ViewBag.Lecturer = claim.Lecturer;
            ViewBag.TotalAmount = claim.TotalAmount;

            return View(claim);
        }

        // 4. CONFIRM PAYMENT → CREATE HrPaymentRecord
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int claimId, string referenceNumber)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer) // ensure lecturer is loaded
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);

            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("ApprovedClaims");
            }

            var record = new HrPaymentRecord
            {
                ClaimID = claim.ClaimID,
                LecturerID = claim.Lecturer.LecturerID,
                Amount = claim.TotalAmount,
                PaidDate = DateTime.Now,
                PaymentReference = referenceNumber
            };

            _context.HrPaymentRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment successfully recorded.";
            return RedirectToAction("ApprovedClaims");
        }


        // 5. EXPORT PAYMENT HISTORY AS CSV
        public async Task<IActionResult> ExportPaymentsCsv()
        {
            var records = await _context.HrPaymentRecords
                .OrderByDescending(r => r.PaidDate)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("PaymentRecordID,ClaimID,LecturerID,Amount,PaidDate,Reference");

            foreach (var r in records)
            {
                sb.AppendLine($"{r.PaymentRecordID},{r.ClaimID},{r.LecturerID},{r.Amount},{r.PaidDate},{r.PaymentReference}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "PaymentHistory.csv");
        }

        // 6. PAYMENT INVOICE (PRINTABLE)
        public async Task<IActionResult> PaymentInvoice(int id)
        {
            var record = await _context.HrPaymentRecords.FirstOrDefaultAsync(r => r.PaymentRecordID == id);
            if (record == null)
            {
                TempData["Error"] = "Payment record not found.";
                return RedirectToAction("Dashboard");
            }

            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.ClaimID == record.ClaimID);

            ViewBag.Claim = claim;
            ViewBag.Lecturer = claim?.Lecturer;

            return View(record);
        }

        // 7. MANAGE LECTURERS
        public async Task<IActionResult> ManageLecturers()
        {
            var lecturers = await _context.Lecturers.OrderBy(l => l.LastName).ToListAsync();
            return View(lecturers);
        }

        // 8. EDIT LECTURER (GET)
        public async Task<IActionResult> EditLecturer(int id)
        {
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == id);
            if (lecturer == null)
            {
                TempData["Error"] = "Lecturer not found.";
                return RedirectToAction("ManageLecturers");
            }

            return View(lecturer);
        }

        // 9. EDIT LECTURER (POST)
        [HttpPost]
        public async Task<IActionResult> EditLecturer(Lecturer lecturer)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid lecturer information.";
                return View(lecturer);
            }

            _context.Lecturers.Update(lecturer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lecturer information updated successfully.";
            return RedirectToAction("ManageLecturers");
        }
    }
}
