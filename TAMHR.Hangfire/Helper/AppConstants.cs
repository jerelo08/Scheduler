namespace TAMHR.Hangfire.Helper
{
    public static class AppConstants
    {
        public static string HangfireConnectionString
        {
            get
            {
                return GetConnectionStringHangFire();
            }
        }
        public const string HangfireSchemaName = "DB_HANGFIRE";
        private static string GetConnectionStringHangFire()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var connectionString = config["ConnectionStrings:HangfireConnection"];
            return connectionString;
        }
        public static string GetAppSetting(string key)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var value = config[key];
            return value;
        }
    }
}
