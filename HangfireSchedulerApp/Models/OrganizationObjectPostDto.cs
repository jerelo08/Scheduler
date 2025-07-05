namespace HangfireSchedulerApp.Models
{
    public class OrganizationObjectPostDto
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
        public string ObjectType { get; set; }
        public string ObjectID { get; set; }
        public string Abbreviation { get; set; }
        public string ObjectText { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ObjectDescription { get; set; }
    }
}
