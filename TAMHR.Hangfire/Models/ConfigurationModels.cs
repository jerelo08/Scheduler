namespace TAMHR.Hangfire.Models
{
    public class SyncConfiguration
    {
        public int BatchSize { get; set; } = 3000;
        public AuthConfiguration Auth { get; set; } = new();
        public ApiEndpoints ApiEndpoints { get; set; } = new();
        public int ApiTimeout { get; set; } = 30;
    }

    public class AuthConfiguration
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ApiEndpoints
    {
        public string Users { get; set; } = string.Empty;
        public string ActualOrg { get; set; } = string.Empty;
        public string ActualEntity { get; set; } = string.Empty;
        public string OrgObject { get; set; } = string.Empty;
        public string EventsCalendar { get; set; } = string.Empty;
    }

    public class ConnectionStrings
    {
        public string HangfireConnection { get; set; } = string.Empty;
        public string DefaultConnection { get; set; } = string.Empty;
        public string EssConnection { get; set; } = string.Empty;
        public string LogConnection { get; set; } = string.Empty;
    }

    public class CronTime
    {
        public string DataSyncJob { get; set; } = "0 3 * * *"; // Default: 3 AM daily Jakarta time
    }
}
