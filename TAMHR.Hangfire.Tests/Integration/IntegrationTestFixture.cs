using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TAMHR.Hangfire.Extension;
using TAMHR.Hangfire.Services;
using TAMHR.Hangfire.Models;
using TAMHR.Hangfire.Schedulers;
using System.IO;

namespace TAMHR.Hangfire.Tests.Integration
{
    public class IntegrationTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public ServiceProvider ConcreteServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }
        
        public IntegrationTestFixture()
        {
            // Build configuration from the actual appsettings.json
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "TAMHR.Hangfire"));
            
            Configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Build service collection
            var services = new ServiceCollection();
            
            // Add configuration to DI container
            services.AddSingleton<IConfiguration>(Configuration);
            
            // Add logging
            services.AddLogging(builder => builder.AddConsole());
            
            // Configure data sync services using the same extension method as the main app
            services.ConfigureDataSyncServices(Configuration);
            
            // Build service provider
            ConcreteServiceProvider = services.BuildServiceProvider();
            ServiceProvider = ConcreteServiceProvider;
        }

        public void Dispose()
        {
            ConcreteServiceProvider?.Dispose();
        }
    }
}
