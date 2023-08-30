using SessionManager.Api.Models;

namespace SessionManager.Api.Services
{
    public class FakeSessionManager : ISessionManager
    {
        private List<Session> _sessions = new List<Session>();

        public FakeSessionManager()
        {
            GenerateRandomSessions();
        }

        public Task<Guid> CreateNewSession(CreateSessionDto createSessionDto)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Name = createSessionDto.Name,
                Description = createSessionDto.Description,
                Date = createSessionDto.Date,
                Location = createSessionDto.Location,
                Speaker = createSessionDto.Speaker,
                SpeakerEmail = createSessionDto.SpeakerEmail
            };
            _sessions.Add(session);
            return Task.FromResult(session.Id);
        }

        public Task<bool> DeleteSession(Guid sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id.Equals(sessionId));
            if (session != null)
            {
                _sessions.Remove(session);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<Session?> GetSessionById(Guid sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id.Equals(sessionId));
            return Task.FromResult(session);
        }

        public Task<List<Session>> GetAllSessions()
        {
            var sessionList = _sessions.ToList();
            return Task.FromResult(sessionList);
        }

        public Task<bool> UpdateExistingSession(Guid sessionId, UpdateSessionDto updateSessionDto)
        {
            var session = _sessions.FirstOrDefault(s => s.Id.Equals(sessionId));
            if (session != null)
            {
                session.Name = updateSessionDto.Name;
                session.Description = updateSessionDto.Description;
                session.Date = updateSessionDto.Date;
                session.Location = updateSessionDto.Location;
                session.Speaker = updateSessionDto.Speaker;
                session.SpeakerEmail = updateSessionDto.SpeakerEmail;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private void GenerateRandomSessions()
        {
            for (int i = 0; i < 10; i++)
            {
                var session = new Session
                {
                    Id = Guid.NewGuid(),
                    Name = $"Session Number: {i}",
                    Description = $"Session Number: {i} will be awesome!",
                    Date = DateTime.UtcNow,
                    Location = $"Conference Room: {i}",
                    Speaker = "Will Velida",
                    SpeakerEmail = "willvelida@hotmail.co.uk"
                };
                _sessions.Add(session);
            }
        }
    }
}
