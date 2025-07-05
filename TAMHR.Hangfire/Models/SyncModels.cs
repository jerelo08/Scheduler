using System.ComponentModel.DataAnnotations;

namespace TAMHR.Hangfire.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string? Email { get; set; }
        public int IsActive { get; set; }
    }

    public class ActualOrganizationStructure
    {
        public Guid Id { get; set; }
        public string OrgCode { get; set; }
        public string? ParentOrgCode { get; set; }
        public string? OrgName { get; set; }
        public string? Service { get; set; }
        public string? NoReg { get; set; }
        public string? Name { get; set; }
        public string? PostCode { get; set; }
        public string? PostName { get; set; }
        public string? JobCode { get; set; }
        public string? JobName { get; set; }
        public string? EmployeeGroup { get; set; }
        public string? EmployeeGroupText { get; set; }
        public string? EmployeeSubgroup { get; set; }
        public string? EmployeeSubgroupText { get; set; }
        public string? WorkContract { get; set; }
        public string? WorkContractText { get; set; }
        public string? PersonalArea { get; set; }
        public string? PersonalSubarea { get; set; }
        public int? DepthLevel { get; set; }
        public decimal? Staffing { get; set; }
        public int? Chief { get; set; }
        public int? OrgLevel { get; set; }
        public int? Vacant { get; set; }
        public string? Structure { get; set; }
        public string? ObjectDescription { get; set; }
    }

    public class ActualEntityStructure
    {
        public Guid Id { get; set; }
        public string OrgCode { get; set; }
        public string ObjectCode { get; set; }
        public string ObjectText { get; set; }
        public string ObjectDescription { get; set; }
    }

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

    public class SchedulerLog
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

    public class TB_R_SYNC_TRACKING
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public DateTime SyncTimestamp { get; set; }
    }
}
