using ContactManager.Api.Models;

namespace ContactManager.Api.Services
{
    public interface IContactsManager
    {
        Task<Contact?> GetContactById(Guid contactId);
        Task<List<Contact>> GetAllContactsByCreator(string createdBy);
        Task<Guid> CreateNewContact(Contact contact);
        Task DeleteContact(Guid contactId);
        Task UpdateContact(Guid contactId, ContactDTO contactDTO);
    }
}
