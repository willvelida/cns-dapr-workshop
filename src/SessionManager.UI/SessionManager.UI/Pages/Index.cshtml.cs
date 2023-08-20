using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SessionManager.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        [BindProperty]
        public string SessionSpeaker { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {
            if (!string.IsNullOrEmpty(SessionSpeaker))
            {
                Response.Cookies.Append("SessionSpeakerCookie", SessionSpeaker);
            }

            return RedirectToPage("./Sessions/Index");
        }
    }
}