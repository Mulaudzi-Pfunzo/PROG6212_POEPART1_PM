namespace CMCS.Models
{
    public class Lecturer
    {
        public int LecturerID { get; set; }           // Primary Key
        public int RoleID { get; set; }               // FK -> UserRole
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string StaffNumber { get; set; } = ""; // Unique staff reference

        public ICollection<Claim>? Claims { get; set; }
    }
}

