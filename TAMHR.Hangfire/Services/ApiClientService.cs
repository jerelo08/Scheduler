using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TAMHR.Hangfire.Models;

namespace TAMHR.Hangfire.Services
{
    public interface IApiClientService
    {
        Task<bool> SendUsersAsync(IEnumerable<User> users);
        Task<bool> SendActualOrgAsync(IEnumerable<ActualOrganizationStructure> actualOrg);
        Task<bool> SendActualEntityAsync(IEnumerable<ActualEntityStructure> actualEntity);
        Task<bool> SendOrgObjectAsync(IEnumerable<OrganizationObject> orgObject);
        Task<bool> SendEventsCalendarAsync(IEnumerable<EventsCalendar> eventsCalendar);
    }

    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly SyncConfiguration _config;
        private readonly ILogger<ApiClientService> _logger;
        private readonly IModelMapper _mapper;
        private readonly ISqlLogService _sqlLogService;

        public ApiClientService(HttpClient httpClient, SyncConfiguration config, ILogger<ApiClientService> logger, IModelMapper mapper, ISqlLogService sqlLogService)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
            _mapper = mapper;
            _sqlLogService = sqlLogService;
            
            _httpClient.Timeout = TimeSpan.FromSeconds(config.ApiTimeout);
            
            // Set up Basic Authentication like in HangfireSchedulerApp
            if (!string.IsNullOrEmpty(config.Auth.Username) && !string.IsNullOrEmpty(config.Auth.Password))
            {
                var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.Auth.Username}:{config.Auth.Password}"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            }
        }

        public async Task<bool> SendUsersAsync(IEnumerable<User> users)
        {
            var dtos = _mapper.MapToDto(users);
            return await SendDataAsync(_config.ApiEndpoints.Users, dtos, "Users");
        }

        public async Task<bool> SendActualOrgAsync(IEnumerable<ActualOrganizationStructure> actualOrg)
        {
            var dtos = _mapper.MapToDto(actualOrg);
            return await SendDataAsync(_config.ApiEndpoints.ActualOrg, dtos, "ActualOrg");
        }

        public async Task<bool> SendActualEntityAsync(IEnumerable<ActualEntityStructure> actualEntity)
        {
            var dtos = _mapper.MapToDto(actualEntity);
            return await SendDataAsync(_config.ApiEndpoints.ActualEntity, dtos, "ActualEntity");
        }

        public async Task<bool> SendOrgObjectAsync(IEnumerable<OrganizationObject> orgObject)
        {
            var dtos = _mapper.MapToDto(orgObject);
            return await SendDataAsync(_config.ApiEndpoints.OrgObject, dtos, "OrgObject");
        }

        public async Task<bool> SendEventsCalendarAsync(IEnumerable<EventsCalendar> eventsCalendar)
        {
            var dtos = _mapper.MapToDto(eventsCalendar);
            return await SendDataAsync(_config.ApiEndpoints.EventsCalendar, dtos, "EventsCalendar");
        }

        private async Task<bool> SendDataAsync<T>(string endpoint, IEnumerable<T> data, string dataType)
        {
            try
            {
                var start = DateTime.Now;
                _logger.LogInformation($"🚀 Start sending to [{dataType}] at {start:HH:mm:ss}");

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation($"➡️ Sending POST to {dataType} ({endpoint})");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseText = await response.Content.ReadAsStringAsync();
                var end = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ [{dataType}] Success - Status: {response.StatusCode} - Time: {(end - start).TotalSeconds}s");
                    await _sqlLogService.WriteLogAsync("TAMHR", "API Call", dataType, "Success", null, $"Response: {responseText}");
                    return true;
                }
                else
                {
                    _logger.LogError($"❌ [{dataType}] Failed - Status: {response.StatusCode} - Time: {(end - start).TotalSeconds}s");
                    await _sqlLogService.WriteLogAsync("TAMHR", "API Call", dataType, "Failed", null, $"Error Response: {responseText}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"🔥 Error processing '{dataType}': {ex.Message}");
                await _sqlLogService.WriteLogAsync("TAMHR.Hangfire", "API Call", dataType, "Error", ex.ToString(), "Exception during job execution");
                return false;
            }
        }
    }
}
