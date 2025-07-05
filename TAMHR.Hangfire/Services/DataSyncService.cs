using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TAMHR.Hangfire.Models;

namespace TAMHR.Hangfire.Services
{
    public interface IDataSyncService
    {
        Task ExecuteSyncAsync();
    }

    public class DataSyncService : IDataSyncService
    {
        private readonly ISyncDataRepository _repository;
        private readonly IApiClientService _apiClient;
        private readonly SyncConfiguration _config;
        private readonly ILogger<DataSyncService> _logger;

        public DataSyncService(
            ISyncDataRepository repository,
            IApiClientService apiClient,
            SyncConfiguration config,
            ILogger<DataSyncService> logger)
        {
            _repository = repository;
            _apiClient = apiClient;
            _config = config;
            _logger = logger;
        }

        public async Task ExecuteSyncAsync()
        {
            _logger.LogInformation("Starting data synchronization job");

            // Execute sync tasks in the specified order
            await SyncUsersAsync();
            await SyncActualOrgAsync();
            await SyncActualEntityAsync();
            await SyncOrgObjectAsync();
            await SyncEventsCalendarAsync();

            _logger.LogInformation("Data synchronization job completed");
        }

        private async Task SyncUsersAsync()
        {
            await SyncEntityAsync<User>(
                "User",
                "Sync Batch: Users",
                async (excludeIds, skip, take) => await _repository.GetUsersAsync(excludeIds, skip, take),
                async (batch) => await _apiClient.SendUsersAsync(batch),
                (user) => user.Id.ToString()
            );
        }

        private async Task SyncActualOrgAsync()
        {
            await SyncEntityAsync<ActualOrganizationStructure>(
                "ActualOrg",
                "Sync Batch: ActualOrg",
                async (excludeIds, skip, take) => await _repository.GetActualOrgAsync(excludeIds, skip, take),
                async (batch) => await _apiClient.SendActualOrgAsync(batch),
                (org) => org.Id.ToString()
            );
        }

        private async Task SyncActualEntityAsync()
        {
            await SyncEntityAsync<ActualEntityStructure>(
                "ActualEntity",
                "Sync Batch: ActualEntity",
                async (excludeIds, skip, take) => await _repository.GetActualEntityAsync(excludeIds, skip, take),
                async (batch) => await _apiClient.SendActualEntityAsync(batch),
                (entity) => entity.Id.ToString()
            );
        }

        private async Task SyncOrgObjectAsync()
        {
            await SyncEntityAsync<OrganizationObject>(
                "OrgObject",
                "Sync Batch: OrgObject",
                async (excludeIds, skip, take) => await _repository.GetOrgObjectAsync(excludeIds, skip, take),
                async (batch) => await _apiClient.SendOrgObjectAsync(batch),
                (obj) => obj.Id.ToString()
            );
        }

        private async Task SyncEventsCalendarAsync()
        {
            await SyncEntityAsync<EventsCalendar>(
                "EventsCalendar",
                "Sync Batch: EventsCalendar",
                async (excludeIds, skip, take) => await _repository.GetEventsCalendarAsync(excludeIds, skip, take),
                async (batch) => await _apiClient.SendEventsCalendarAsync(batch),
                (calendar) => calendar.Id.ToString()
            );
        }

        private async Task SyncEntityAsync<T>(
            string entityType,
            string activityName,
            Func<IEnumerable<string>, int, int, Task<IEnumerable<T>>> fetchData,
            Func<IEnumerable<T>, Task<bool>> sendData,
            Func<T, string> getId)
        {
            try
            {
                _logger.LogInformation($"Starting sync for {entityType}");

                // Get already synced entity IDs
                var syncedIds = await _repository.GetSyncedEntityIdsAsync(entityType);
                var syncedIdsList = syncedIds.ToList();

                _logger.LogInformation($"Found {syncedIdsList.Count} already synced {entityType} records");

                var skip = 0;
                var totalProcessed = 0;

                while (true)
                {
                    // Fetch data batch
                    var data = await fetchData(syncedIdsList, skip, _config.BatchSize);
                    var dataList = data.ToList();

                    if (!dataList.Any())
                    {
                        _logger.LogInformation($"No more {entityType} records to process");
                        break;
                    }

                    _logger.LogInformation($"Processing batch of {dataList.Count} {entityType} records");

                    var stopwatch = Stopwatch.StartNew();
                    bool success = false;
                    string? errorMessage = null;
                    HttpStatusCode? statusCode = null;

                    try
                    {
                        // Send data to API
                        success = await sendData(dataList);
                        statusCode = success ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        errorMessage = ex.Message;
                        _logger.LogError(ex, $"Error sending {entityType} batch");
                    }

                    stopwatch.Stop();

                    // Log the activity
                    await LogActivity(activityName, success, dataList.Count, stopwatch.ElapsedMilliseconds, statusCode, errorMessage);

                    if (success)
                    {
                        // Mark records as synced
                        var entityIds = dataList.Select(getId).ToList();
                        await _repository.AddSyncTrackingAsync(entityType, entityIds);
                        
                        totalProcessed += dataList.Count;
                        _logger.LogInformation($"Successfully synced batch of {dataList.Count} {entityType} records");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to sync batch of {dataList.Count} {entityType} records, will retry in next run");
                    }

                    // Move to next batch
                    skip += _config.BatchSize;
                }

                _logger.LogInformation($"Completed sync for {entityType}. Total processed: {totalProcessed}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during {entityType} sync");
                await LogActivity($"{activityName} - Error", false, 0, 0, null, ex.Message);
            }
        }

        private async Task LogActivity(string activity, bool success, int batchSize, long durationMs, HttpStatusCode? statusCode, string? errorMessage)
        {
            var log = new SchedulerLog
            {
                ID = Guid.NewGuid(),
                ApplicationName = "TAMHR.Hangfire",
                LogID = Guid.NewGuid().ToString(),
                LogCategory = "DataSync",
                Activity = activity,
                ApplicationModule = "DataSyncService",
                IPHostName = Environment.MachineName,
                Status = success ? "Success" : "Failure",
                AdditionalInformation = JsonSerializer.Serialize(new
                {
                    BatchSize = batchSize,
                    DurationMs = durationMs,
                    HttpResponseCode = (int?)statusCode,
                    Timestamp = DateTime.UtcNow
                }),
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = "System",
                ModifiedOn = DateTime.UtcNow,
                RowStatus = true,
                ExceptionMessage = errorMessage
            };

            try
            {
                await _repository.LogActivityAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log activity");
            }
        }
    }
}
