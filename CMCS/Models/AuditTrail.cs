using System.ComponentModel.DataAnnotations;

namespace CMCS.Models
{
    public class AuditTrail
    {
        [Key]
        public int AuditID { get; set; }              // Primary Key
        public int ClaimID { get; set; }              // FK -> Claim
        public int RoleID { get; set; }               // FK -> UserRole
        public string ActionType { get; set; } = "";  // e.g. Submitted, Approved, Rejected
        public DateTime ActionDate { get; set; }      // Date of the action
        public string Comments { get; set; } = "";    // Optional notes 
    }
}

