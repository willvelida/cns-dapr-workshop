using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SessionManager.UI.Pages.Sessions.Models;

namespace SessionManager.UI.Pages.Sessions
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<Session?> SessionsList { get; set; }

        [BindProperty]
        public string? SessionSpeaker { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            SessionSpeaker = Request.Cookies["SessionSpeakerCookie"];
            var httpClient = _httpClientFactory.CreateClient("BackendUrl");
            SessionsList = await httpClient.GetFromJsonAsync<List<Session>>($"/sessions?speakerName={SessionSpeaker}");
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid sessionId)
        {
            var httpClient = _httpClientFactory.CreateClient("BackendUrl");
            var result = await httpClient.DeleteAsync($"api/sessions/{sessionId}");
            return RedirectToPage();
        }
    }
}
