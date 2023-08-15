using Bogus;
using ContactManager.Api.Models;

namespace ContactManager.Api.Services
{
    public class FakeContactsManager : IContactsManager
    {
        private List<Contact> _contacts = new List<Contact>();

        private void GenerateRandomContacts()
        {
            for (int i = 0; i < 10; i++)
            {
                var contact = new Faker<Contact>()
                    .RuleFor(c => c.ContactId, f => Guid.NewGuid())
                    .RuleFor(c => c.Name, f => f.Name.FullName())
                    .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Name))
                    .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber())
                    .RuleFor(c => c.ContactCreatedBy, f => "willvelida@hotmail.co.uk")
                    .RuleFor(c => c.ContactCreatedOn, f => DateTime.UtcNow)
                    .RuleFor(c => c.ContactUpdatedOn, f => DateTime.UtcNow)
                    .Generate();
                _contacts.Add(contact);
            }
        }

        public FakeContactsManager()
        {
            GenerateRandomContacts();
        }

        public Task<Guid> CreateNewContact(Contact contact)
        {
            var newContact = new Contact
            {
                ContactId = Guid.NewGuid(),
                Name = contact.Name,
                Email = contact.Email,
                PhoneNumber = contact.PhoneNumber,
                ContactCreatedBy = contact.ContactCreatedBy,
                ContactUpdatedOn = DateTime.UtcNow,
                ContactCreatedOn = DateTime.UtcNow,
            };

            _contacts.Add(newContact);
            return Task.FromResult(newContact.ContactId);
        }

        public Task DeleteContact(Guid contactId)
        {
            var contact = _contacts.FirstOrDefault(c => c.ContactId.Equals(contactId));
            if (contact is not null)
            {
                _contacts.Remove(contact);
            }
            return Task.FromResult(0);
        }

        public Task<List<Contact>> GetAllContactsByCreator(string createdBy)
        {
            var contacts = _contacts.Where(c => c.ContactCreatedBy.Equals(createdBy)).ToList();
            return Task.FromResult(contacts);
        }

        public Task<Contact?> GetContactById(Guid contactId)
        {
            var contact = _contacts.FirstOrDefault(c => c.ContactId.Equals(contactId));
            return Task.FromResult(contact);
        }

        public Task UpdateContact(Guid contactId, ContactDTO contactDTO)
        {
            var contact = _contacts.FirstOrDefault(c => c.ContactId.Equals(contactId));
            if (contact is not null)
            {
                contact.Name = contactDTO.Name;
                contact.Email = contactDTO.Email;
                contact.PhoneNumber = contactDTO.PhoneNumber;
                contact.ContactUpdatedOn = DateTime.UtcNow;
                return Task.FromResult(contact);
            }
            return Task.FromResult(0);
        }
    }
}
