using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TAMHR.Hangfire.Extension;
using TAMHR.Hangfire.Schedulers;
using TAMHR.Hangfire.Services;
using TAMHR.Hangfire.Models;
using Hangfire;
using Hangfire.SqlServer;
using System;
using System.Threading.Tasks;

namespace TAMHR.Hangfire.Tests.Manual
{
    /// <summary>
    /// Manual test program to verify Hangfire job registration and basic functionality
    /// Run this to test the system without starting the full web application
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🧪 TAMHR.Hangfire Manual Test Program");
            Console.WriteLine("=====================================\n");

            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("../../../TAMHR.Hangfire/appsettings.json", optional: false)
                    .Build();

                // Build service collection
                var services = new ServiceCollection();
                
                // Add logging
                services.AddLogging(builder => 
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Configure Hangfire
                services.ConfigureHangfire();
                
                // Configure data sync services
                services.ConfigureDataSyncServices(configuration);

                // Build service provider
                var serviceProvider = services.BuildServiceProvider();

                Console.WriteLine("✅ Service configuration completed");

                // Test 1: Verify all services are registered
                await TestServiceRegistration(serviceProvider);

                // Test 2: Test configuration loading
                await TestConfiguration(serviceProvider);

                // Test 3: Test database connections
                await TestDatabaseConnections(configuration);

                // Test 4: Test DataSyncJob instantiation
                await TestDataSyncJob(serviceProvider);

                // Test 5: Test individual services
                await TestIndividualServices(serviceProvider);

                Console.WriteLine("\n🎉 All manual tests completed successfully!");
                Console.WriteLine("\n💡 To test Hangfire dashboard and job scheduling:");
                Console.WriteLine("   1. Run: dotnet run --project TAMHR.Hangfire");
                Console.WriteLine("   2. Open: http://localhost:5000/hangfire");
                Console.WriteLine("   3. Check 'Recurring Jobs' tab for 'DataSyncJob'");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during manual testing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static async Task TestServiceRegistration(IServiceProvider serviceProvider)
        {
            Console.WriteLine("\n🔧 Testing Service Registration...");

            var services = new[]
            {
                typeof(IDataSyncService),
                typeof(IApiClientService),
                typeof(ISyncDataRepository),
                typeof(IModelMapper),
                typeof(ISqlLogService),
                typeof(DataSyncJob),
                typeof(SyncConfiguration)
            };

            foreach (var serviceType in services)
            {
                var service = serviceProvider.GetService(serviceType);
                if (service != null)
                {
                    Console.WriteLine($"   ✅ {serviceType.Name} - Registered");
                }
                else
                {
                    Console.WriteLine($"   ❌ {serviceType.Name} - NOT Registered");
                }
            }
        }

        static async Task TestConfiguration(IServiceProvider serviceProvider)
        {
            Console.WriteLine("\n⚙️  Testing Configuration...");

            var syncConfig = serviceProvider.GetService<SyncConfiguration>();
            if (syncConfig != null)
            {
                Console.WriteLine($"   ✅ BatchSize: {syncConfig.BatchSize}");
                Console.WriteLine($"   ✅ ApiTimeout: {syncConfig.ApiTimeout}");
                Console.WriteLine($"   ✅ Username: {syncConfig.Auth?.Username ?? "NOT SET"}");
                Console.WriteLine($"   ✅ Password: {(string.IsNullOrEmpty(syncConfig.Auth?.Password) ? "NOT SET" : "SET")}");
                Console.WriteLine($"   ✅ Users Endpoint: {syncConfig.ApiEndpoints?.Users ?? "NOT SET"}");
            }
            else
            {
                Console.WriteLine("   ❌ SyncConfiguration not loaded");
            }
        }

        static async Task TestDatabaseConnections(IConfiguration configuration)
        {
            Console.WriteLine("\n🔌 Testing Database Connections...");

            var connections = new[]
            {
                ("DefaultConnection", configuration.GetConnectionString("DefaultConnection")),
                ("EssConnection", configuration.GetConnectionString("EssConnection")),
                ("LogConnection", configuration.GetConnectionString("LogConnection")),
                ("HangfireConnection", configuration.GetConnectionString("HangfireConnection"))
            };

            foreach (var (name, connectionString) in connections)
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    try
                    {
                        using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                        await connection.OpenAsync();
                        Console.WriteLine($"   ✅ {name} - Connected");
                        await connection.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ {name} - Failed: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"   ❌ {name} - Connection string not configured");
                }
            }
        }

        static async Task TestDataSyncJob(IServiceProvider serviceProvider)
        {
            Console.WriteLine("\n📋 Testing DataSyncJob...");

            try
            {
                var dataSyncJob = serviceProvider.GetRequiredService<DataSyncJob>();
                Console.WriteLine("   ✅ DataSyncJob instantiated successfully");

                // Don't actually execute the job in manual test, just verify it can be created
                Console.WriteLine("   ✅ DataSyncJob ready for execution");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ DataSyncJob failed: {ex.Message}");
            }
        }

        static async Task TestIndividualServices(IServiceProvider serviceProvider)
        {
            Console.WriteLine("\n🔍 Testing Individual Services...");

            // Test ModelMapper
            try
            {
                var mapper = serviceProvider.GetRequiredService<IModelMapper>();
                var testUser = new User
                {
                    Id = Guid.NewGuid(),
                    NoReg = "TEST001",
                    Username = "testuser",
                    Name = "Test User",
                    Gender = "M",
                    Email = "test@example.com",
                    IsActive = 1
                };

                var userDto = mapper.MapToDto(testUser);
                Console.WriteLine($"   ✅ ModelMapper - User DTO mapped (ImportType: {userDto.ImportType})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ ModelMapper failed: {ex.Message}");
            }

            // Test SqlLogService
            try
            {
                var logService = serviceProvider.GetRequiredService<ISqlLogService>();
                await logService.WriteLogAsync("TAMHR.Test", "Manual Test", "TestActivity", "Success", null, "Manual test log entry");
                Console.WriteLine("   ✅ SqlLogService - Log written successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ SqlLogService failed: {ex.Message}");
            }

            // Test SyncDataRepository
            try
            {
                var repository = serviceProvider.GetRequiredService<ISyncDataRepository>();
                var syncedIds = await repository.GetSyncedEntityIdsAsync("ManualTest");
                Console.WriteLine($"   ✅ SyncDataRepository - Retrieved {syncedIds.Count()} synced IDs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ SyncDataRepository failed: {ex.Message}");
            }
        }
    }
}
