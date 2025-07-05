namespace HangfireSchedulerApp.Models
{
    public class OrganizationObject
    {
        public Guid Id { get; set; }
        public string ObjectID { get; set; }
        public string ObjectType { get; set; }
        public string Abbreviation { get; set; }
        public string? ObjectText { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ObjectDescription { get; set; }
    }
}
