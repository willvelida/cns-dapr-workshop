using Refit;
using SessionManager.UI.Models;

namespace SessionManager.UI.Services
{
    public interface ISessionManagerService
    {
        [Get("/sessions")]
        Task<List<Session>> GetAllSessions();
        [Get("/sessions/{sessionId}")]
        Task<Session?> GetSessionById(Guid sessionId);
        [Post("/sessions")]
        Task<Guid> CreateNewSession([Body] CreateSessionDto createSessionDto);
        [Put("/sessions/{sessionId}")]
        Task<bool> UpdateExistingSession(Guid sessionId, UpdateSessionDto updateSessionDto);
        [Delete("/sessions/{sessionId}")]
        Task<bool> DeleteSession(Guid sessionId);
    }
}
