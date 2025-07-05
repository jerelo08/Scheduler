using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TAMHR.Hangfire.Services;
using TAMHR.Hangfire.Models;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TAMHR.Hangfire.Tests.Integration
{
    public class DatabaseIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public DatabaseIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task DefaultConnection_ShouldConnect_Successfully()
        {
            // Arrange
            var connectionString = _fixture.Configuration.GetConnectionString("DefaultConnection");
            
            // Act & Assert
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            var result = await connection.QuerySingleAsync<int>("SELECT 1");
            Assert.Equal(1, result);
            
            _output.WriteLine("✅ DefaultConnection: Successfully connected to database");
        }

        [Fact]
        public async Task EssConnection_ShouldConnect_Successfully()
        {
            // Arrange
            var connectionString = _fixture.Configuration.GetConnectionString("EssConnection");
            
            // Act & Assert
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            var result = await connection.QuerySingleAsync<int>("SELECT 1");
            Assert.Equal(1, result);
            
            _output.WriteLine("✅ EssConnection: Successfully connected to database");
        }

        [Fact]
        public async Task LogConnection_ShouldConnect_Successfully()
        {
            // Arrange
            var connectionString = _fixture.Configuration.GetConnectionString("LogConnection");
            
            // Act & Assert
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            var result = await connection.QuerySingleAsync<int>("SELECT 1");
            Assert.Equal(1, result);
            
            _output.WriteLine("✅ LogConnection: Successfully connected to database");
        }

        [Fact]
        public async Task HangfireConnection_ShouldConnect_Successfully()
        {
            // Arrange
            var connectionString = _fixture.Configuration.GetConnectionString("HangfireConnection");
            
            // Act & Assert
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            var result = await connection.QuerySingleAsync<int>("SELECT 1");
            Assert.Equal(1, result);
            
            _output.WriteLine("✅ HangfireConnection: Successfully connected to database");
        }

        [Fact]
        public async Task SyncTracking_Table_ShouldExist()
        {
            // Arrange
            var connectionString = _fixture.Configuration.GetConnectionString("LogConnection");
            
            // Act & Assert
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            var tableExists = await connection.QuerySingleAsync<int>(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'TB_R_SYNC_TRACKING'");
            
            Assert.True(tableExists > 0, "TB_R_SYNC_TRACKING table should exist in LogConnection database");
            
            _output.WriteLine("✅ TB_R_SYNC_TRACKING table exists in LogConnection database");
        }

        [Fact]
        public async Task TB_R_Log_Table_ShouldExist()
        {
            // Arrange
            var connectionString = _fixture.Configuration.GetConnectionString("LogConnection");
            
            // Act & Assert
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            var tableExists = await connection.QuerySingleAsync<int>(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'TB_R_Log'");
            
            Assert.True(tableExists > 0, "TB_R_Log table should exist in LogConnection database");
            
            _output.WriteLine("✅ TB_R_Log table exists in LogConnection database");
        }

        [Fact]
        public async Task SyncDataRepository_ShouldFetchUsers_Successfully()
        {
            // Arrange
            var repository = _fixture.ServiceProvider.GetRequiredService<ISyncDataRepository>();
            
            // Act
            var users = await repository.GetUsersAsync(new List<string>(), 0, 10);
            
            // Assert
            Assert.NotNull(users);
            _output.WriteLine($"✅ Successfully fetched {users.Count()} users from database");
        }

        [Fact]
        public async Task SyncDataRepository_ShouldFetchActualOrg_Successfully()
        {
            // Arrange
            var repository = _fixture.ServiceProvider.GetRequiredService<ISyncDataRepository>();
            
            // Act
            var actualOrgs = await repository.GetActualOrgAsync(new List<string>(), 0, 10);
            
            // Assert
            Assert.NotNull(actualOrgs);
            _output.WriteLine($"✅ Successfully fetched {actualOrgs.Count()} ActualOrg records from database");
        }

        [Fact]
        public async Task SyncDataRepository_ShouldFetchEventsCalendar_Successfully()
        {
            // Arrange
            var repository = _fixture.ServiceProvider.GetRequiredService<ISyncDataRepository>();
            
            // Act
            var events = await repository.GetEventsCalendarAsync(new List<string>(), 0, 10);
            
            // Assert
            Assert.NotNull(events);
            _output.WriteLine($"✅ Successfully fetched {events.Count()} EventsCalendar records from ESS database");
        }

        [Fact]
        public async Task SqlLogService_ShouldWriteLog_Successfully()
        {
            // Arrange
            var logService = _fixture.ServiceProvider.GetRequiredService<ISqlLogService>();
            
            // Act
            await logService.WriteLogAsync(
                "TAMHR.Hangfire.Tests", 
                "Integration Test", 
                "Test Log Entry", 
                "Success", 
                null, 
                "Integration test verification");
            
            // Assert - Verify log was written
            var connectionString = _fixture.Configuration.GetConnectionString("LogConnection");
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            var logCount = await connection.QuerySingleAsync<int>(@"
                SELECT COUNT(*) 
                FROM TB_R_Log 
                WHERE ApplicationName = 'TAMHR.Hangfire.Tests' 
                AND Activity = 'Test Log Entry'
                AND CreatedOn >= DATEADD(minute, -1, GETDATE())");
            
            Assert.True(logCount > 0, "Log entry should be written to TB_R_Log table");
            
            _output.WriteLine("✅ Successfully wrote log entry to TB_R_Log table");
        }

        [Fact]
        public async Task SyncTracking_ShouldAddAndRetrieve_Successfully()
        {
            // Arrange
            var repository = _fixture.ServiceProvider.GetRequiredService<ISyncDataRepository>();
            var testEntityType = "IntegrationTest";
            var testEntityIds = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            
            // Act - Add tracking entries
            var addResult = await repository.AddSyncTrackingAsync(testEntityType, testEntityIds);
            Assert.True(addResult, "Should successfully add sync tracking entries");
            
            // Act - Retrieve tracking entries
            var retrievedIds = await repository.GetSyncedEntityIdsAsync(testEntityType);
            var retrievedList = retrievedIds.ToList();
            
            // Assert
            Assert.True(retrievedList.Count >= testEntityIds.Count, "Should retrieve at least the added entities");
            Assert.True(testEntityIds.All(id => retrievedList.Contains(id)), "All added entity IDs should be retrievable");
            
            _output.WriteLine($"✅ Successfully added and retrieved {testEntityIds.Count} sync tracking entries");
        }
    }
}
