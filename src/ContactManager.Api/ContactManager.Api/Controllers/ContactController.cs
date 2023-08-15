using ContactManager.Api.Models;
using ContactManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.Api.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;
        private readonly IContactsManager _contactsManager;

        public ContactController(ILogger<ContactController> logger, IContactsManager contactsManager)
        {
            _logger = logger;
            _contactsManager = contactsManager;
        }

        [HttpGet]
        public async Task<IEnumerable<Contact>> Get(string createdBy)
        {
            return await _contactsManager.GetAllContactsByCreator(createdBy);
        }

        [HttpGet("{contactId}")]
        public async Task<IActionResult> GetContact(Guid contactId)
        {
            var contact = await _contactsManager.GetContactById(contactId);
            if (contact is not null)
            {
                return Ok(contact);
            }
            return NotFound();
        }

        [HttpPut("{contactId}")]
        public async Task<IActionResult> Put(Guid contactId, [FromBody] ContactDTO contactDTO)
        {
            await _contactsManager.UpdateContact(contactId, contactDTO);
            return Ok();
        }

        [HttpDelete("{contactId}")]
        public async Task<IActionResult> Delete(Guid contactId)
        {
            await _contactsManager.DeleteContact(contactId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Contact contact)
        {
            await _contactsManager.CreateNewContact(contact);
            return Created($"/api/contacts/{contact.ContactId}", null);
        }
    }
}
