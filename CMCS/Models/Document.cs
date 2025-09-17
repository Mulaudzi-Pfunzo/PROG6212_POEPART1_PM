namespace CMCS.Models
{
    public class Document
    {
        public int DocumentID { get; set; }          // Primary Key
        public int ClaimID { get; set; }             // FK -> Claim
        public string FileName { get; set; } = "";   // Name of the uploaded file
        public string FileType { get; set; } = "";   // e.g. PDF, JPG
        public string FilePath { get; set; } = "";   // Storage location
        public DateTime UploadDate { get; set; }     // When the document was uploaded
    }
}

