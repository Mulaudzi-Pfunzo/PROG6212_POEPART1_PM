using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.Models
{
    public class Claim
    {
        public int ClaimID { get; set; }               // Primary Key
        public int LecturerID { get; set; }            // FK -> Lecturer
        public double HoursWorked { get; set; }        // Number of hours claimed

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }        // Hourly pay rate

        public DateTime ClaimDate { get; set; }        // Date of submission
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [NotMapped]
        public decimal TotalAmount => (decimal)HoursWorked * HourlyRate; // Calculated amount

        // =========================
        // NAVIGATION PROPERTY (Fix)
        // =========================
        public Lecturer? Lecturer { get; set; }         // <-- Add this
    }
}