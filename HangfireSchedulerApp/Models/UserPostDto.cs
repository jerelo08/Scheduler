namespace HangfireSchedulerApp.Models
{
    public class UserPostDto
    {
        public int ImportType { get; set; } = 1;
        public int ImportStatus_ID { get; set; } = 1;
        public string BatchTag { get; set; } = "Tag";
        public int ErrorCode { get; set; } = 200;

        public string Code { get; set; }               // → NoReg
        public string Name { get; set; }               // → Name
        public string NewCode { get; set; }            // → Username
        public string UniqueCode { get; set; }         // → NoReg
        public string created_at_system { get; set; }  // → DateTime.Now (or from DB if ada)

        public string ID { get; set; }                 // → Id (Guid → string)
        public string NoReg { get; set; }              // → NoReg
        public string Username { get; set; }           // → Username
        public string Email { get; set; }              // → Email
        public string CreatedOn { get; set; }          // → DateTime.Now
        public string CreatedBy { get; set; }          // → "System"
        public string LastLogin { get; set; }          // → ""
        public string IsActive { get; set; }           // → IsActive.ToString()
    }
}
