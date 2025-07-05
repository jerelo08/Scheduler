namespace TAMHR.Hangfire.Service.Config.Email
{
    public class auth
    {
        public static string from = appConfig.i.GetSection("Smtp").GetSection("Username").Value;
        public static string host = appConfig.i.GetSection("Smtp").GetSection("Server").Value;
        public static string password = appConfig.i.GetSection("Smtp").GetSection("Password").Value;
        public static int port = int.Parse(appConfig.i.GetSection("Smtp").GetSection("Port").Value);
    }
}
