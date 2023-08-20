using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SessionManager.UI.Pages.Sessions.Models;

namespace SessionManager.UI.Pages.Sessions
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public UpdateSessionDto? UpdateSessionDto { get; set; }

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(Guid? sessionId)
        {
            if (sessionId == null)
            {
                return NotFound();
            }

            var httpClient = _httpClientFactory.CreateClient("BackendUrl");
            var session = await httpClient.GetFromJsonAsync<Session>($"api/sessions/{sessionId}");

            if (session == null)
            {
                return NotFound();
            }

            UpdateSessionDto = new UpdateSessionDto
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                Start = session.Start,
                End = session.End,
                Location = session.Location,
                Speaker = session.Speaker,
                SpeakerEmail = session.SpeakerEmail,
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (UpdateSessionDto != null)
            {
                var httpClient = _httpClientFactory.CreateClient("BackendUrl");
                var result = await httpClient.PutAsJsonAsync($"api/sessions/{UpdateSessionDto.Id}", UpdateSessionDto);
            }

            return RedirectToPage("./Index");
        }
    }
}
