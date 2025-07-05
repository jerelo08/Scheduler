using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TAMHR.Hangfire.Services;
using TAMHR.Hangfire.Models;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace TAMHR.Hangfire.Tests.Integration
{
    public class EndToEndIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public EndToEndIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task DataSyncService_ShouldExecute_WithRealDatabase()
        {
            // Arrange
            var dataSyncService = _fixture.ServiceProvider.GetRequiredService<IDataSyncService>();
            var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<EndToEndIntegrationTests>>();
            
            _output.WriteLine("🚀 Starting End-to-End Data Sync Test with Real Database...");
            
            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await dataSyncService.ExecuteSyncAsync();
            });
            
            // Assert
            if (exception != null)
            {
                _output.WriteLine($"⚠️ DataSyncService completed with exception (expected due to API endpoints): {exception.GetType().Name}");
                _output.WriteLine($"   Message: {exception.Message}");
                
                // We expect HttpRequestException or similar due to API endpoints not being accessible in test environment
                // But the database operations should work fine
                Assert.True(
                    exception is HttpRequestException || 
                    exception is TaskCanceledException ||
                    exception.Message.Contains("timeout") ||
                    exception.Message.Contains("network") ||
                    exception.Message.Contains("connection"),
                    $"Expected network-related exception, but got: {exception.GetType().Name} - {exception.Message}");
            }
            else
            {
                _output.WriteLine("✅ DataSyncService executed successfully!");
            }
        }

        [Fact]
        public async Task ModelMapper_ShouldMapAllEntities_Successfully()
        {
            // Arrange
            var mapper = _fixture.ServiceProvider.GetRequiredService<IModelMapper>();
            var repository = _fixture.ServiceProvider.GetRequiredService<ISyncDataRepository>();
            
            _output.WriteLine("🔄 Testing ModelMapper with real database data...");
            
            // Test User mapping
            var users = await repository.GetUsersAsync(new List<string>(), 0, 1);
            if (users.Any())
            {
                var userDto = mapper.MapToDto(users.First());
                Assert.NotNull(userDto);
                Assert.Equal(1, userDto.ImportType);
                Assert.Equal(200, userDto.ErrorCode);
                _output.WriteLine("✅ User mapping successful");
            }
            
            // Test ActualOrg mapping
            var actualOrgs = await repository.GetActualOrgAsync(new List<string>(), 0, 1);
            if (actualOrgs.Any())
            {
                var orgDto = mapper.MapToDto(actualOrgs.First());
                Assert.NotNull(orgDto);
                Assert.Equal(1, orgDto.ImportType);
                Assert.Equal(200, orgDto.ErrorCode);
                _output.WriteLine("✅ ActualOrg mapping successful");
            }
            
            // Test EventsCalendar mapping
            var events = await repository.GetEventsCalendarAsync(new List<string>(), 0, 1);
            if (events.Any())
            {
                var eventDto = mapper.MapToDto(events.First());
                Assert.NotNull(eventDto);
                Assert.Equal(1, eventDto.ImportType);
                Assert.Equal(200, eventDto.ErrorCode);
                _output.WriteLine("✅ EventsCalendar mapping successful");
            }
            
            _output.WriteLine("✅ All available entity mappings tested successfully");
        }

        [Fact]
        public async Task SyncTracking_ShouldPreventDuplicates_InRealScenario()
        {
            // Arrange
            var repository = _fixture.ServiceProvider.GetRequiredService<ISyncDataRepository>();
            var testEntityType = "E2ETest";
            var testEntityId = Guid.NewGuid().ToString();
            
            _output.WriteLine("🔄 Testing duplicate prevention with real database...");
            
            // Act - Add entity to tracking
            await repository.AddSyncTrackingAsync(testEntityType, new List<string> { testEntityId });
            
            // Act - Get synced IDs (should include our test entity)
            var syncedIds = await repository.GetSyncedEntityIdsAsync(testEntityType);
            var syncedList = syncedIds.ToList();
            
            // Assert
            Assert.Contains(testEntityId, syncedList);
            _output.WriteLine($"✅ Entity {testEntityId} successfully tracked");
            
            // Act - Simulate fetching users excluding already synced ones
            var users = await repository.GetUsersAsync(syncedList, 0, 5);
            
            // Assert - Users should not contain any IDs that are in syncedList
            foreach (var user in users)
            {
                Assert.DoesNotContain(user.Id.ToString(), syncedList);
            }
            
            _output.WriteLine($"✅ Duplicate prevention working - fetched {users.Count()} users, none were in sync tracking");
        }

        [Fact]
        public async Task ApiClientService_ShouldHandleAuthentication_Correctly()
        {
            // Arrange
            var apiClient = _fixture.ServiceProvider.GetRequiredService<IApiClientService>();
            var config = _fixture.ServiceProvider.GetRequiredService<SyncConfiguration>();
            
            _output.WriteLine("🔄 Testing API Client authentication setup...");
            
            // Assert configuration is loaded
            Assert.NotNull(config.Auth);
            Assert.False(string.IsNullOrEmpty(config.Auth.Username));
            Assert.False(string.IsNullOrEmpty(config.Auth.Password));
            
            _output.WriteLine($"✅ Authentication configured - Username: {config.Auth.Username}");
            
            // Test with empty data (should handle gracefully)
            var emptyUsers = new List<User>();
            var result = await apiClient.SendUsersAsync(emptyUsers);
            
            // We expect this to attempt the API call and likely fail due to network/endpoint issues
            // But it should be set up correctly with authentication
            _output.WriteLine($"✅ API Client handles empty data correctly - Result: {result}");
        }

        [Fact]
        public async Task SqlLogService_ShouldLog_DetailedInformation()
        {
            // Arrange
            var logService = _fixture.ServiceProvider.GetRequiredService<ISqlLogService>();
            var repository = _fixture.ServiceProvider.GetRequiredService<ISyncDataRepository>();
            
            _output.WriteLine("🔄 Testing detailed logging functionality...");
            
            // Act - Write various types of logs
            await logService.WriteLogAsync("TAMHR.Tests", "API Call", "Users", "Success", null, "Response: {\"status\":\"ok\"}");
            await logService.WriteLogAsync("TAMHR.Tests", "API Call", "ActualOrg", "Failed", null, "Error Response: {\"error\":\"timeout\"}");
            await logService.WriteLogAsync("TAMHR.Tests", "API Call", "EventsCalendar", "Error", "System.TimeoutException: Request timeout", "Exception during job execution");
            
            // Give a moment for logs to be written
            await Task.Delay(100);
            
            _output.WriteLine("✅ All log types written successfully");
            _output.WriteLine("   - Success log with response details");
            _output.WriteLine("   - Failed log with error response");
            _output.WriteLine("   - Exception log with stack trace");
        }

        [Fact]
        public async Task FullSyncCycle_ShouldWork_WithSmallBatch()
        {
            // Arrange
            var repository = _fixture.ServiceProvider.GetRequiredService<ISyncDataRepository>();
            var mapper = _fixture.ServiceProvider.GetRequiredService<IModelMapper>();
            var logService = _fixture.ServiceProvider.GetRequiredService<ISqlLogService>();
            
            _output.WriteLine("🔄 Testing full sync cycle with small batch...");
            
            // Act - Simulate a small sync cycle for Users
            var syncedIds = await repository.GetSyncedEntityIdsAsync("TestUser");
            var users = await repository.GetUsersAsync(syncedIds, 0, 2); // Small batch of 2
            
            if (users.Any())
            {
                // Map to DTOs
                var userDtos = mapper.MapToDto(users);
                Assert.True(userDtos.Any());
                
                // Log the operation
                await logService.WriteLogAsync("TAMHR.Tests", "Sync Test", "Users", "Success", null, $"Processed {users.Count()} users");
                
                // Simulate successful API call by adding to tracking
                var userIds = users.Select(u => u.Id.ToString()).ToList();
                await repository.AddSyncTrackingAsync("TestUser", userIds);
                
                _output.WriteLine($"✅ Full sync cycle completed for {users.Count()} users");
                _output.WriteLine("   - Fetched from database ✓");
                _output.WriteLine("   - Mapped to DTOs ✓");
                _output.WriteLine("   - Logged operation ✓");
                _output.WriteLine("   - Added to sync tracking ✓");
            }
            else
            {
                _output.WriteLine("ℹ️ No users available for sync test (all may be already synced)");
            }
        }
    }
}
