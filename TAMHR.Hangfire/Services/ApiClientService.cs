using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TAMHR.Hangfire.Models;

namespace TAMHR.Hangfire.Services
{
    public interface IApiClientService
    {
        Task<HttpStatusCode> SendUsersAsync(IEnumerable<User> users);
        Task<HttpStatusCode> SendActualOrgAsync(IEnumerable<ActualOrganizationStructure> actualOrg);
        Task<HttpStatusCode> SendActualEntityAsync(IEnumerable<ActualEntityStructure> actualEntity);
        Task<HttpStatusCode> SendOrgObjectAsync(IEnumerable<OrganizationObject> orgObject);
        Task<HttpStatusCode> SendEventsCalendarAsync(IEnumerable<EventsCalendar> eventsCalendar);
    }

    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly SyncConfiguration _config;
        private readonly ILogger<ApiClientService> _logger;
        private readonly IModelMapper _mapper;

        public ApiClientService(
            HttpClient httpClient, 
            SyncConfiguration config, 
            ILogger<ApiClientService> logger, 
            IModelMapper mapper)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
            _mapper = mapper;
            
            _httpClient.Timeout = TimeSpan.FromSeconds(config.ApiTimeout);
            
            // Set up Basic Authentication like in HangfireSchedulerApp
            if (!string.IsNullOrEmpty(config.Auth.Username) && !string.IsNullOrEmpty(config.Auth.Password))
            {
                var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.Auth.Username}:{config.Auth.Password}"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            }
        }

        public async Task<HttpStatusCode> SendUsersAsync(IEnumerable<User> users)
        {
            var dtos = _mapper.MapToDto(users);
            return await SendDataAsync(_config.ApiEndpoints.Users, dtos, "Users");
        }

        public async Task<HttpStatusCode> SendActualOrgAsync(IEnumerable<ActualOrganizationStructure> actualOrg)
        {
            var dtos = _mapper.MapToDto(actualOrg);
            return await SendDataAsync(_config.ApiEndpoints.ActualOrg, dtos, "ActualOrg");
        }

        public async Task<HttpStatusCode> SendActualEntityAsync(IEnumerable<ActualEntityStructure> actualEntity)
        {
            var dtos = _mapper.MapToDto(actualEntity);
            return await SendDataAsync(_config.ApiEndpoints.ActualEntity, dtos, "ActualEntity");
        }

        public async Task<HttpStatusCode> SendOrgObjectAsync(IEnumerable<OrganizationObject> orgObject)
        {
            var dtos = _mapper.MapToDto(orgObject);
            return await SendDataAsync(_config.ApiEndpoints.OrgObject, dtos, "OrgObject");
        }

        public async Task<HttpStatusCode> SendEventsCalendarAsync(IEnumerable<EventsCalendar> eventsCalendar)
        {
            var dtos = _mapper.MapToDto(eventsCalendar);
            return await SendDataAsync(_config.ApiEndpoints.EventsCalendar, dtos, "EventsCalendar");
        }

        private async Task<HttpStatusCode> SendDataAsync<T>(string endpoint, IEnumerable<T> data, string dataType)
        {
            try
            {
                var start = DateTime.UtcNow;
                _logger.LogInformation($"🚀 Start sending to [{dataType}] at UTC {start:HH:mm:ss}");

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation($"➡️ Sending POST to {dataType} ({endpoint})");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseText = await response.Content.ReadAsStringAsync();
                var end = DateTime.UtcNow;

                if (response == null)
                {
                    throw new ArgumentException("Response is NULL");
                }

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ [{dataType}] Success - Status: {response.StatusCode} - Time: {(end - start).TotalSeconds}s");
                    return response.StatusCode;
                }
                else
                {
                    _logger.LogError($"❌ [{dataType}] Failed - Status: {response.StatusCode} - Time: {(end - start).TotalSeconds}s");
                    return response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"🔥 Error processing '{dataType}': {ex.Message}");

                // Remember this is caused by Exception, not by Actual API Response
                return HttpStatusCode.UnprocessableEntity;
            }
        }
    }
}
