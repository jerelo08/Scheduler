using Hangfire.SqlServer;
using Hangfire;
using TAMHR.Hangfire.Helper;
using TAMHR.Hangfire.Services;
using TAMHR.Hangfire.Models;
using TAMHR.Hangfire.Schedulers;

namespace TAMHR.Hangfire.Extension
{
    internal static class ServiceExtension
    {
        internal static void ConfigureHangfire(this IServiceCollection services)
        {
            services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseMediatR());

            JobStorage.Current = new SqlServerStorage(AppConstants.HangfireConnectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                SchemaName = AppConstants.HangfireSchemaName
            });
        }

        internal static void ConfigureDataSyncServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register configuration objects
            var syncConfig = configuration.GetSection("SyncConfiguration").Get<SyncConfiguration>() ?? new SyncConfiguration();
            services.AddSingleton(syncConfig);

            // Register HTTP client for API calls
            services.AddHttpClient<IApiClientService, ApiClientService>();

            // Register data sync services
            services.AddScoped<ISyncDataRepository, SyncDataRepository>();
            services.AddScoped<IModelMapper, ModelMapper>();
            services.AddScoped<IApiClientService, ApiClientService>();
            services.AddScoped<IDataSyncService, DataSyncService>();
            services.AddScoped<ISqlLogService, SqlLogService>();
            services.AddScoped<DataSyncJob>();
        }
    }
}
