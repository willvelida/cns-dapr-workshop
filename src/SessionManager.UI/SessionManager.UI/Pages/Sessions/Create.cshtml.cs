using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SessionManager.UI.Pages.Sessions.Models;

namespace SessionManager.UI.Pages.Sessions
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public CreateSessionDto CreateSessionDto { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (CreateSessionDto != null)
            {
                var sessionSpeaker = Request.Cookies["SessionSpeakerCookie"];

                CreateSessionDto.Speaker = sessionSpeaker;

                var httpClient = _httpClientFactory.CreateClient("BackendUrl");
                var result = await httpClient.PostAsJsonAsync("api/sessions", CreateSessionDto);
            }
            return RedirectToPage("./Index");
        }
    }
}
