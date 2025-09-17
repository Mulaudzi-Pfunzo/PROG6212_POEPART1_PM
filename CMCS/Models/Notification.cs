namespace CMCS.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }      // Primary Key
        public int ClaimID { get; set; }             // FK -> Claim
        public int LecturerID { get; set; }          // FK -> Lecturer
        public string Message { get; set; } = "";    // Notification text
        public DateTime DateSent { get; set; }       // When it was sent
        public string Status { get; set; } = "Unread"; // Read / Unread
    }
}
