namespace HangfireSchedulerApp.Models
{
    public class EventsCalendar
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string EventTypeCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}
