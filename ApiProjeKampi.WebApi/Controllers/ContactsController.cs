using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.ContactDtos;
using ApiProjeKampi.WebApi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly ApiContext _contex;
         public ContactsController(ApiContext contex)
        {
            _contex = contex;
        }
        [HttpGet]
        public IActionResult ContactList() 
        {
            var values = _contex.Contacts.ToList();
            return Ok(values);
        }
        [HttpPost]
        public IActionResult CreateContact(CreateContactDto createContactDto)
        {
            Contact contact = new Contact();
            contact.Email = createContactDto.Email; 
            contact.Adress = createContactDto.Adress;
            contact.Phone = createContactDto.Phone; 
            contact.MapLocation = createContactDto.MapLocation;
            contact.OpenHours = createContactDto.OpenHours;
            _contex.Contacts.Add(contact);
            _contex.SaveChanges();
            return Ok("Ekleme işlemi başarılı.");
        }
        [HttpDelete]
        public IActionResult DeleteContact(int id)
        {
            var value = _contex.Contacts.Find(id);
            _contex.Contacts.Remove(value);
            _contex.SaveChanges();
            return Ok("Silme işlemi başarılı.");
        }
        [HttpGet("GetContact")]
        public IActionResult GetContact(int id)
        {
            var value = _contex.Contacts.Find(id);
            return Ok(value);
        }
        [HttpPut]
        public IActionResult UpdateContact(UpdateContactDto updateContactDto)
        {
            Contact contact = new Contact();
            contact.Email = updateContactDto.Email;
            contact.Adress = updateContactDto.Adress;
            contact.Phone = updateContactDto.Phone;
            contact.ContactId = updateContactDto.ContactId;
            contact.MapLocation = updateContactDto.MapLocation;
            contact.OpenHours = updateContactDto.OpenHours;
            _contex.Contacts.Update(contact);
            _contex.SaveChanges();
            return Ok("Güncelleme başarılı");
        }
    }
}
