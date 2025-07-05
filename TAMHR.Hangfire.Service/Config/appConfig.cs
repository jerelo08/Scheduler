using Microsoft.Extensions.Configuration;
using TAMHR.Hangfire.Domain;

namespace TAMHR.Hangfire.Service.Config
{
    public class appConfig
    {
        public static IConfigurationRoot i = new AppConfiguration().root;
    }
}
