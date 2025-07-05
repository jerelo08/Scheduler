namespace HangfireSchedulerApp.Models
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
}
