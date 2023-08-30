namespace SessionManager.Api.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public string Speaker { get; set; }
        public string SpeakerEmail { get; set; }
    }
}
