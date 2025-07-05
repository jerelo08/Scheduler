using Moq;
using Microsoft.Extensions.Logging;
using TAMHR.Hangfire.Services;
using TAMHR.Hangfire.Models;
using Xunit;

namespace TAMHR.Hangfire.Tests.Services
{
    public class DataSyncServiceTests
    {
        private readonly Mock<ISyncDataRepository> _mockRepository;
        private readonly Mock<IApiClientService> _mockApiClient;
        private readonly Mock<ILogger<DataSyncService>> _mockLogger;
        private readonly SyncConfiguration _config;
        private readonly DataSyncService _service;

        public DataSyncServiceTests()
        {
            _mockRepository = new Mock<ISyncDataRepository>();
            _mockApiClient = new Mock<IApiClientService>();
            _mockLogger = new Mock<ILogger<DataSyncService>>();
            _config = new SyncConfiguration { BatchSize = 2 }; // Small batch size for testing

            _service = new DataSyncService(
                _mockRepository.Object,
                _mockApiClient.Object,
                _config,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ExecuteSyncAsync_Should_Execute_All_Sync_Tasks()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetSyncedEntityIdsAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<string>());

            _mockRepository.Setup(r => r.GetUsersAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<User>());

            _mockRepository.Setup(r => r.GetActualOrgAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<ActualOrganizationStructure>());

            _mockRepository.Setup(r => r.GetActualEntityAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<ActualEntityStructure>());

            _mockRepository.Setup(r => r.GetOrgObjectAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<OrganizationObject>());

            _mockRepository.Setup(r => r.GetEventsCalendarAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<EventsCalendar>());

            // Act
            await _service.ExecuteSyncAsync();

            // Assert
            _mockRepository.Verify(r => r.GetSyncedEntityIdsAsync("User"), Times.Once);
            _mockRepository.Verify(r => r.GetSyncedEntityIdsAsync("ActualOrg"), Times.Once);
            _mockRepository.Verify(r => r.GetSyncedEntityIdsAsync("ActualEntity"), Times.Once);
            _mockRepository.Verify(r => r.GetSyncedEntityIdsAsync("OrgObject"), Times.Once);
            _mockRepository.Verify(r => r.GetSyncedEntityIdsAsync("EventsCalendar"), Times.Once);
        }

        [Fact]
        public async Task SyncUsersAsync_Should_Exclude_Already_Synced_Records()
        {
            // Arrange
            var syncedIds = new List<string> { "guid1", "guid2" };
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user1" },
                new User { Id = Guid.NewGuid(), Username = "user2" }
            };

            _mockRepository.Setup(r => r.GetSyncedEntityIdsAsync("User"))
                          .ReturnsAsync(syncedIds);

            _mockRepository.Setup(r => r.GetUsersAsync(syncedIds, 0, 2))
                          .ReturnsAsync(users);

            _mockRepository.Setup(r => r.GetUsersAsync(syncedIds, 2, 2))
                          .ReturnsAsync(new List<User>());

            _mockApiClient.Setup(a => a.SendUsersAsync(It.IsAny<IEnumerable<User>>()))
                         .ReturnsAsync(System.Net.HttpStatusCode.OK);

            _mockRepository.Setup(r => r.AddSyncTrackingAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                          .ReturnsAsync(true);

            _mockRepository.Setup(r => r.LogActivityAsync(It.IsAny<SchedulerLog>()))
                          .ReturnsAsync(true);

            // Act
            await _service.ExecuteSyncAsync();

            // Assert
            _mockRepository.Verify(r => r.GetUsersAsync(syncedIds, It.IsAny<int>(), It.IsAny<int>()), Times.AtLeastOnce);
            _mockApiClient.Verify(a => a.SendUsersAsync(users), Times.Once);
            _mockRepository.Verify(r => r.AddSyncTrackingAsync("User", It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task SyncUsersAsync_Should_Process_Data_In_Batches()
        {
            // Arrange
            var users1 = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user1" },
                new User { Id = Guid.NewGuid(), Username = "user2" }
            };

            var users2 = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user3" }
            };

            _mockRepository.Setup(r => r.GetSyncedEntityIdsAsync("User"))
                          .ReturnsAsync(new List<string>());

            _mockRepository.Setup(r => r.GetUsersAsync(It.IsAny<IEnumerable<string>>(), 0, 2))
                          .ReturnsAsync(users1);

            _mockRepository.Setup(r => r.GetUsersAsync(It.IsAny<IEnumerable<string>>(), 2, 2))
                          .ReturnsAsync(users2);

            _mockRepository.Setup(r => r.GetUsersAsync(It.IsAny<IEnumerable<string>>(), 4, 2))
                          .ReturnsAsync(new List<User>());

            // Setup other entities to return empty
            SetupEmptyResponses();

            _mockApiClient.Setup(a => a.SendUsersAsync(It.IsAny<IEnumerable<User>>()))
                         .ReturnsAsync(System.Net.HttpStatusCode.OK);

            _mockRepository.Setup(r => r.AddSyncTrackingAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                          .ReturnsAsync(true);

            _mockRepository.Setup(r => r.LogActivityAsync(It.IsAny<SchedulerLog>()))
                          .ReturnsAsync(true);

            // Act
            await _service.ExecuteSyncAsync();

            // Assert
            _mockApiClient.Verify(a => a.SendUsersAsync(users1), Times.Once);
            _mockApiClient.Verify(a => a.SendUsersAsync(users2), Times.Once);
            _mockRepository.Verify(r => r.AddSyncTrackingAsync("User", It.IsAny<IEnumerable<string>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task SyncUsersAsync_Should_Not_Mark_As_Synced_When_API_Fails()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user1" }
            };

            _mockRepository.Setup(r => r.GetSyncedEntityIdsAsync("User"))
                          .ReturnsAsync(new List<string>());

            _mockRepository.Setup(r => r.GetUsersAsync(It.IsAny<IEnumerable<string>>(), 0, 2))
                          .ReturnsAsync(users);

            _mockRepository.Setup(r => r.GetUsersAsync(It.IsAny<IEnumerable<string>>(), 2, 2))
                          .ReturnsAsync(new List<User>());

            // Setup other entities to return empty
            SetupEmptyResponses();

            _mockApiClient.Setup(a => a.SendUsersAsync(It.IsAny<IEnumerable<User>>()))
                         .ReturnsAsync(System.Net.HttpStatusCode.InternalServerError); // Simulate API failure

            _mockRepository.Setup(r => r.LogActivityAsync(It.IsAny<SchedulerLog>()))
                          .ReturnsAsync(true);

            // Act
            await _service.ExecuteSyncAsync();

            // Assert
            _mockApiClient.Verify(a => a.SendUsersAsync(users), Times.Once);
            _mockRepository.Verify(r => r.AddSyncTrackingAsync("User", It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        private void SetupEmptyResponses()
        {
            _mockRepository.Setup(r => r.GetSyncedEntityIdsAsync("ActualOrg"))
                          .ReturnsAsync(new List<string>());
            _mockRepository.Setup(r => r.GetSyncedEntityIdsAsync("ActualEntity"))
                          .ReturnsAsync(new List<string>());
            _mockRepository.Setup(r => r.GetSyncedEntityIdsAsync("OrgObject"))
                          .ReturnsAsync(new List<string>());
            _mockRepository.Setup(r => r.GetSyncedEntityIdsAsync("EventsCalendar"))
                          .ReturnsAsync(new List<string>());

            _mockRepository.Setup(r => r.GetActualOrgAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<ActualOrganizationStructure>());
            _mockRepository.Setup(r => r.GetActualEntityAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<ActualEntityStructure>());
            _mockRepository.Setup(r => r.GetOrgObjectAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<OrganizationObject>());
            _mockRepository.Setup(r => r.GetEventsCalendarAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<EventsCalendar>());
        }
    }
}
