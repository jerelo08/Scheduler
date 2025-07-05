using Hangfire.SqlServer;
using Hangfire;
using TAMHR.Hangfire.Helper;

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
    }
}
