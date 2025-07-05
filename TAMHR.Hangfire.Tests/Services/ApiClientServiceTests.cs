using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using TAMHR.Hangfire.Services;
using TAMHR.Hangfire.Models;
using System.Net;
using System.Text;
using Xunit;

namespace TAMHR.Hangfire.Tests.Services
{
    public class ApiClientServiceTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<ApiClientService>> _mockLogger;
        private readonly Mock<IModelMapper> _mockMapper;
        private readonly HttpClient _httpClient;
        private readonly SyncConfiguration _config;
        private readonly ApiClientService _service;

        public ApiClientServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<ApiClientService>>();
            _mockMapper = new Mock<IModelMapper>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            
            _config = new SyncConfiguration
            {
                Auth = new AuthConfiguration
                {
                    Username = "testuser",
                    Password = "testpass"
                },
                ApiEndpoints = new ApiEndpoints
                {
                    Users = "https://api.example.com/users",
                    ActualOrg = "https://api.example.com/actualorg",
                    ActualEntity = "https://api.example.com/actualentity",
                    OrgObject = "https://api.example.com/orgobject",
                    EventsCalendar = "https://api.example.com/eventscalendar"
                },
                ApiTimeout = 30
            };

            _service = new ApiClientService(_httpClient, _config, _mockLogger.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task SendUsersAsync_Should_Return_True_When_API_Call_Succeeds()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "testuser" }
            };

            var userDtos = new List<UserPostDto>
            {
                new UserPostDto { Username = "testuser" }
            };

            _mockMapper.Setup(m => m.MapToDto(users)).Returns(userDtos);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Success", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _service.SendUsersAsync(users);

            // Assert
            Assert.True(result == HttpStatusCode.OK);
            _mockMapper.Verify(m => m.MapToDto(users), Times.Once);
        }

        [Fact]
        public async Task SendUsersAsync_Should_Return_False_When_API_Call_Fails()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "testuser" }
            };

            var userDtos = new List<UserPostDto>
            {
                new UserPostDto { Username = "testuser" }
            };

            _mockMapper.Setup(m => m.MapToDto(users)).Returns(userDtos);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Error", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _service.SendUsersAsync(users);

            // Assert
            Assert.False(result == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SendUsersAsync_Should_Return_False_When_Exception_Occurs()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "testuser" }
            };

            var userDtos = new List<UserPostDto>
            {
                new UserPostDto { Username = "testuser" }
            };

            _mockMapper.Setup(m => m.MapToDto(users)).Returns(userDtos);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _service.SendUsersAsync(users);

            // Assert
            Assert.False(result == HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task SendActualOrgAsync_Should_Call_Correct_Endpoint()
        {
            // Arrange
            var actualOrg = new List<ActualOrganizationStructure>
            {
                new ActualOrganizationStructure { Id = Guid.NewGuid(), OrgCode = "ORG001" }
            };

            var actualOrgDtos = new List<ActualOrgPostDto>
            {
                new ActualOrgPostDto { OrgCode = "ORG001" }
            };

            _mockMapper.Setup(m => m.MapToDto(actualOrg)).Returns(actualOrgDtos);

            HttpRequestMessage? capturedRequest = null;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, token) =>
                {
                    capturedRequest = request;
                })
                .ReturnsAsync(responseMessage);

            // Act
            await _service.SendActualOrgAsync(actualOrg);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(_config.ApiEndpoints.ActualOrg, capturedRequest.RequestUri?.ToString());
            Assert.Equal(HttpMethod.Post, capturedRequest.Method);
        }

        [Fact]
        public async Task SendEventsCalendarAsync_Should_Handle_Large_Batch()
        {
            // Arrange
            var events = Enumerable.Range(1, 1000)
                .Select(i => new EventsCalendar
                {
                    Id = Guid.NewGuid(),
                    Title = $"Event {i}",
                    EventTypeCode = "TYPE001",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddHours(1),
                    CreatedBy = "System",
                    CreatedOn = DateTime.Now,
                    RowStatus = true
                })
                .ToList();

            var eventDtos = Enumerable.Range(1, 1000)
                .Select(i => new EventsCalendarPostDto
                {
                    ID = Guid.NewGuid().ToString(),
                    EventTypeCode = "TYPE001"
                })
                .ToList();

            _mockMapper.Setup(m => m.MapToDto(events)).Returns(eventDtos);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _service.SendEventsCalendarAsync(events);

            // Assert
            Assert.True(result == HttpStatusCode.OK);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
