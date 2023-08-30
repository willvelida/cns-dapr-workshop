using Refit;
using SessionManager.UI.Models;
using System.Text.Json;

namespace SessionManager.UI.Services
{
    public class SessionManagerService : ISessionManagerService
    {
        IHttpClientFactory _httpClientFactory;
        RefitSettings _settings;

        public SessionManagerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };

            _settings = new RefitSettings()
            {
                ContentSerializer = new SystemTextJsonContentSerializer(options)
            };
        }

        public async Task<Guid> CreateNewSession([Body] CreateSessionDto createSessionDto)
        {
            var client = _httpClientFactory.CreateClient("Sessions");
            return await RestService.For<ISessionManagerService>(client, _settings).CreateNewSession(createSessionDto);
        }

        public async Task<bool> DeleteSession(Guid sessionId)
        {
            var client = _httpClientFactory.CreateClient("Sessions");
            return await RestService.For<ISessionManagerService>(client, _settings).DeleteSession(sessionId);
        }

        public async Task<List<Session>> GetAllSessions()
        {
            var client = _httpClientFactory.CreateClient("Sessions");
            return await RestService.For<ISessionManagerService>(client, _settings).GetAllSessions();
        }

        public async Task<Session?> GetSessionById(Guid sessionId)
        {
            var client = _httpClientFactory.CreateClient("Sessions");
            return await RestService.For<ISessionManagerService>(client, _settings).GetSessionById(sessionId);
        }

        public async Task<bool> UpdateExistingSession(Guid sessionId, UpdateSessionDto updateSessionDto)
        {
            var client = _httpClientFactory.CreateClient("Sessions");
            return await RestService.For<ISessionManagerService>(client, _settings).UpdateExistingSession(sessionId, updateSessionDto);
        }
    }
}
