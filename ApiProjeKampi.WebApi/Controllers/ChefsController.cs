using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChefsController : ControllerBase
    {
        private readonly ApiContext _contex;
        public ChefsController(ApiContext contex)
        {
            _contex = contex;
        }
        [HttpGet]
        public IActionResult ChefList() 
        {
            var values = _contex.Chefs.ToList();
            return Ok(values);

        }

        [HttpPost]
        public IActionResult CreateChef(Chef chef)
        {
            _contex.Chefs.Add(chef);
            _contex.SaveChanges();
            return Ok("Şef sisteme başarıyla eklendi");
        }
        [HttpDelete]
        public IActionResult DeleteChef(int id) 
        {
            var value = _contex.Chefs.Find(id);
            _contex.Chefs.Remove(value);
            _contex.SaveChanges() ;
            return Ok("Şef sistemden silindi.");

        }
        [HttpGet("GetChef")]
        public IActionResult GetChef(int id)
        {
            return Ok(_contex.Chefs.Find(id));
        }
        [HttpPut]
        public IActionResult UpdateChef(Chef chef)
        {
            _contex.Chefs.Update(chef);
            _contex.SaveChanges();
            return Ok("Güncelleme başarılı");
        }


    }
}
