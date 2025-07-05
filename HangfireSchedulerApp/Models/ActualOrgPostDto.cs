namespace HangfireSchedulerApp.Models
{
    public class ActualOrgPostDto
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
        public string OrgCode { get; set; }
        public string ParentOrgCode { get; set; }
        public string OrgName { get; set; }
        public string Service { get; set; }
        public string NoReg { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string EmployeeGroup { get; set; }
        public string EmployeeGroupText { get; set; }
        public string EmployeeSubgroup { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public string WorkContract { get; set; }
        public string WorkContractText { get; set; }
        public string PersonalArea { get; set; }
        public string PersonalSubarea { get; set; }
        public string DepthLevel { get; set; }
        public string Staffing { get; set; }
        public string Chief { get; set; }
        public string OrgLevel { get; set; }
        public DateTime? Period { get; set; }
        public string Vacant { get; set; }
        public string Structure { get; set; }
        public string ObjectDescription { get; set; }
    }
}
