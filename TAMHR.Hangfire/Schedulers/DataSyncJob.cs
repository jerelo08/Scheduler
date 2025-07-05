using TAMHR.Hangfire.Services;

namespace TAMHR.Hangfire.Schedulers
{
    public class DataSyncJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DataSyncJob> _logger;

        public DataSyncJob(IServiceScopeFactory scopeFactory, ILogger<DataSyncJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void ExecutesSync()
        {
            _logger.LogInformation("DataSyncJob started at {StartTime}", DateTime.UtcNow);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dataSyncService = scope.ServiceProvider.GetRequiredService<IDataSyncService>();
                
                dataSyncService.ExecuteSync();
                
                _logger.LogInformation("DataSyncJob completed successfully at {EndTime}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DataSyncJob failed at {FailTime}", DateTime.UtcNow);
                throw; // Re-throw to let Hangfire handle the failure
            }
        }
    }
}
