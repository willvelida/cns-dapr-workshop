using SessionManager.Api.Models;

namespace SessionManager.Api.Services
{
    public interface ISessionManager
    {
        Task<List<Session>> GetAllSessions();
        Task<Session?> GetSessionById(Guid sessionId);
        Task<Guid> CreateNewSession(CreateSessionDto createSessionDto);
        Task<bool> UpdateExistingSession(Guid sessionId, UpdateSessionDto updateSessionDto);
        Task<bool> DeleteSession(Guid sessionId);
    }
}
