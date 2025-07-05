namespace HangfireSchedulerApp.Models
{
    public class EventsCalendarPostDto
    {
        public int ImportType { get; set; } = 1;
        public int ImportStatus_ID { get; set; } = 1;
        public string BatchTag { get; set; } = "Tag";
        public int ErrorCode { get; set; } = 200;

        public string Code { get; set; }
        public string Name { get; set; }
        public string NewCode { get; set; }
        public string UniqueCode { get; set; }
        public string created_at_system { get; set; }

        public string ID { get; set; }
        public string EventTypeCode { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
        public string RowStatus { get; set; }
    }
}
