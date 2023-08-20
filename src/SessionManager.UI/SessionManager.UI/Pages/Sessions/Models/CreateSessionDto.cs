using System.ComponentModel.DataAnnotations;

namespace SessionManager.UI.Pages.Sessions.Models
{
    public class CreateSessionDto
    {
        [Display(Name = "Session Name")]
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }

        [Display(Name = "Session Start Time")]
        [Required]
        public DateTime Start { get; set; }

        [Display(Name = "Session End Time")]
        [Required]
        public DateTime End { get; set; }
        public string? Location { get; set; }

        [Display(Name = "Speaker Name")]
        [Required]
        public string Speaker { get; set; }

        [Display(Name = "Speaker Email")]
        [Required]
        public string SpeakerEmail { get; set; }
    }
}
