using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using HangfireSchedulerApp.Data;
using HangfireSchedulerApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HangfireSchedulerApp.Services
{
    public class SchedulerService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;
        private readonly EssDbContext _essDb;
        private readonly IServiceProvider _serviceProvider;

        public SchedulerService(HttpClient httpClient, IConfiguration configuration, AppDbContext db, EssDbContext essDb, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _db = db;
            _essDb = essDb;
            _serviceProvider = serviceProvider;
        }

        public async Task RunJobAsync()
        {
            Console.WriteLine($"[Scheduler] Job triggered at {DateTime.Now:HH:mm:ss}");

            var username = _configuration["SchedulerConfig:Auth:Username"];
            var password = _configuration["SchedulerConfig:Auth:Password"];
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var apiConfigs = _configuration.GetSection("SchedulerConfig:ApiUrls").GetChildren();

            using (var scope = _serviceProvider.CreateScope())
            {
                var logService = scope.ServiceProvider.GetRequiredService<SqlLogService>();

                foreach (var entry in apiConfigs)
                {
                    var key = entry.Key;
                    var url = entry.Value;
                    StringContent jsonPayload;

                    try
                    {
                        var start = DateTime.Now;
                        Console.WriteLine($"\n🚀 Start sending to [{key}] at {start:HH:mm:ss}");

                        switch (key)
                        {
                            case "User":
                                var users = await _db.Users.ToListAsync();
                                var userPayload = users.Select(user => new UserPostDto
                                {
                                    ImportType = 1,
                                    ImportStatus_ID = 1,
                                    BatchTag = "Tag",
                                    ErrorCode = 200,
                                    Code = user.Id.ToString(),
                                    Name = user.Name,
                                    NewCode = user.Username,
                                    UniqueCode = user.NoReg,
                                    created_at_system = DateTime.Now.ToString("s"),
                                    ID = "",
                                    NoReg = user.NoReg,
                                    Username = user.Username,
                                    Email = user.Email ?? "",
                                    CreatedOn = DateTime.Now.ToString("yyyy-MM-dd"),
                                    CreatedBy = "System",
                                    LastLogin = "",
                                    IsActive = user.IsActive == 1 ? "1" : "0"
                                }).ToList();
                                jsonPayload = ToJsonPayload(userPayload);
                                break;

                            case "ActualOrg":
                                var orgs = await _db.ActualOrganizationStructures.ToListAsync();
                                var orgPayload = orgs.Select(o => new ActualOrgPostDto
                                {
                                    Code = o.Id.ToString(),
                                    Name = o.OrgName,
                                    OrgCode = o.OrgCode,
                                    ParentOrgCode = o.ParentOrgCode,
                                    Service = o.Service,
                                    NoReg = o.NoReg,
                                    PostCode = o.PostCode,
                                    PostName = o.PostName,
                                    JobCode = o.JobCode,
                                    JobName = o.JobName,
                                    EmployeeGroup = o.EmployeeGroup,
                                    created_at_system = DateTime.Now.ToString("s"),
                                    ImportType = 1,
                                    ImportStatus_ID = 1,
                                    BatchTag = "Tag",
                                    ErrorCode = 200,
                                    ID = ""
                                }).ToList();
                                jsonPayload = ToJsonPayload(orgPayload);
                                break;

                            case "ActualEntity":
                                var entities = await _db.ActualEntityStructures.ToListAsync();
                                var entityPayload = entities.Select(e => new ActualEntityPostDto
                                {
                                    Code = e.Id.ToString(),
                                    Name = e.ObjectText,
                                    OrgCode = e.OrgCode,
                                    ObjectCode = e.ObjectCode,
                                    ObjectText = e.ObjectText,
                                    ObjectDescription = e.ObjectDescription,
                                    created_at_system = DateTime.Now.ToString("s"),
                                    ImportType = 1,
                                    ImportStatus_ID = 1,
                                    BatchTag = "Tag",
                                    ErrorCode = 200,
                                    ID = ""
                                }).ToList();
                                jsonPayload = ToJsonPayload(entityPayload);
                                break;

                            case "OrgObject":
                                var objects = await _db.OrganizationObjects.ToListAsync();
                                var objectPayload = objects.Select(o => new OrganizationObjectPostDto
                                {
                                    Code = o.Id.ToString(),
                                    ObjectType = o.ObjectType,
                                    ObjectID = o.ObjectID,
                                    Abbreviation = o.Abbreviation,
                                    ObjectText = o.ObjectText,
                                    StartDate = o.StartDate.ToString("yyyy-MM-dd"),
                                    EndDate = o.EndDate.ToString("yyyy-MM-dd"),
                                    ObjectDescription = o.ObjectDescription,
                                    created_at_system = DateTime.Now.ToString("s"),
                                    ImportType = 1,
                                    ImportStatus_ID = 1,
                                    BatchTag = "Tag",
                                    ErrorCode = 200,
                                    ID = ""
                                }).ToList();
                                jsonPayload = ToJsonPayload(objectPayload);
                                break;

                            case "EventsCalendar":
                                var events = await _essDb.EventsCalendars.ToListAsync();
                                var eventPayload = events.Select(e => new EventsCalendarPostDto
                                {
                                    Code = e.Id.ToString(),
                                    Name = e.Title,
                                    EventTypeCode = e.EventTypeCode,
                                    StartDate = e.StartDate.ToString("yyyy-MM-dd"),
                                    EndDate = e.EndDate.ToString("yyyy-MM-dd"),
                                    Description = e.Description,
                                    CreatedBy = e.CreatedBy,
                                    CreatedOn = e.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss"),
                                    ModifiedBy = e.ModifiedBy,
                                    ModifiedOn = e.ModifiedOn?.ToString("yyyy-MM-dd HH:mm:ss"),
                                    RowStatus = e.RowStatus ? "1" : "0",
                                    created_at_system = DateTime.Now.ToString("s"),
                                    ImportType = 1,
                                    ImportStatus_ID = 1,
                                    BatchTag = "Tag",
                                    ErrorCode = 200,
                                    ID = ""
                                }).ToList();
                                jsonPayload = ToJsonPayload(eventPayload);
                                break;

                            default:
                                Console.WriteLine($"⚠️ Endpoint '{key}' tidak dikenal. Melewati.");
                                continue;
                        }

                        Console.WriteLine($"➡️ Sending POST to {key} ({url})");

                        var response = await _httpClient.PostAsync(url, jsonPayload);
                        var responseText = await response.Content.ReadAsStringAsync();
                        var end = DateTime.Now;

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"✅ [{key}] Success - Status: {response.StatusCode} - Time: {(end - start).TotalSeconds}s");
                            await logService.WriteLogAsync("TAMHR", "API Call", key, "Success", null, $"Response: {responseText}");
                        }
                        else
                        {
                            Console.WriteLine($"❌ [{key}] Failed - Status: {response.StatusCode} - Time: {(end - start).TotalSeconds}s");
                            await logService.WriteLogAsync("TAMHR", "API Call", key, "Failed", null, $"Error Response: {responseText}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"🔥 Error processing '{key}': {ex.Message}");
                        await logService.WriteLogAsync("HangfireSchedulerApp", "API Call", key, "Error", ex.ToString(), "Exception during job execution");
                    }
                }
            }
        }

        private static StringContent ToJsonPayload<T>(T payload)
        {
            return new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        }

        public Task RunJobWrapperAsync() => RunJobAsync();
    }
}
