using Microsoft.AspNetCore.Mvc;
using SessionManager.Api.Models;
using SessionManager.Api.Services;

namespace SessionManager.Api.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly ISessionManager _sessionManager;

        public SessionController(ILogger<SessionController> logger, ISessionManager sessionManager)
        {
            _logger = logger;
            _sessionManager = sessionManager;
        }

        [HttpGet]
        public async Task<IEnumerable<Session>> Get()
        {
            return await _sessionManager.GetAllSessions();
        }

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetSession(Guid sessionId)
        {
            var session = await _sessionManager.GetSessionById(sessionId);
            if (session != null)
            {
                return Ok(session);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateSessionDto createSessionDto)
        {
            var sessionId = await _sessionManager.CreateNewSession(createSessionDto);
            return Created($"/api/sessions/{sessionId}", null);
        }

        [HttpPut("{sessionId}")]
        public async Task<IActionResult> Put(Guid sessionId, [FromBody] UpdateSessionDto updateSessionDto)
        {
            var updated = await _sessionManager.UpdateExistingSession(sessionId, updateSessionDto);
            if (updated)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> Delete(Guid sessionId)
        {
            var deleted = await _sessionManager.DeleteSession(sessionId);
            if (deleted)
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
