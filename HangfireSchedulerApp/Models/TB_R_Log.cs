namespace HangfireSchedulerApp.Models
{
    public class TB_R_Log
    {
        public Guid ID { get; set; }
        public string ApplicationName { get; set; }
        public string LogID { get; set; }
        public string LogCategory { get; set; }
        public string Activity { get; set; }
        public string ApplicationModule { get; set; }
        public string IPHostName { get; set; }
        public string Status { get; set; }
        public string? AdditionalInformation { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string? ExceptionMessage { get; set; }
    }
}
