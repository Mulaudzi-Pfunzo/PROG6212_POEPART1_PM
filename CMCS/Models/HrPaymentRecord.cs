namespace CMCS.Models
{
    public class HrPaymentRecord
    {
        public int PaymentRecordID { get; set; }
        public int ClaimID { get; set; }
        public int LecturerID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidDate { get; set; }
        public string PaymentReference { get; set; } = "";
    }
}
