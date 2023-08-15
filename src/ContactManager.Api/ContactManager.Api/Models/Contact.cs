namespace ContactManager.Api.Models
{
    public class Contact
    {
        public Guid ContactId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ContactCreatedBy { get; set; }
        public DateTime ContactCreatedOn { get; set; }
        public DateTime ContactUpdatedOn { get; set; }
    }
}
