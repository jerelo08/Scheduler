using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Domain
{
    public class AppConfiguration
    {
        private readonly string _connectionString = string.Empty;
        private readonly string _applicationLoginType = string.Empty;
        public IConfigurationRoot root;

        public AppConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var _root = configurationBuilder.Build();
            root = _root;
            _connectionString = root.GetSection("ConnectionStrings").GetSection("HangfireConnection").Value;
            //_applicationLoginType = root.GetSection("Authentication").GetSection("ApplicationLoginType").Value;

        }
        public string ConnectionString
        {
            get => _connectionString;
        }

        //public string ApplicationLoginType
        //{
        //    get => _applicationLoginType;
        //}
    }
}
