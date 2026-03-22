using ApiProjeKampi.WebApi.Context;
using ApiProjeKampi.WebApi.Dtos.ImagesDtos;
using ApiProjeKampi.WebApi.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiProjeKampi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ApiContex _context;
        private readonly IMapper _mapper;
        public ImagesController(ApiContex contex, IMapper mapper)
        {
            _context = contex;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult ImageList()
        {
            var values = _context.Images.ToList();
            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateImage(CreateImageDto createImageDto)
        {
            // _context.Images.Add(Image);
            //_context.SaveChanges();
            var value = _mapper.Map<Image>(createImageDto);
            _context.Images.Add(value);
            _context.SaveChanges();
            return Ok("Görsel ekleme işlemi başarılı");
        }
        [HttpDelete]
        public IActionResult DeleteImage(int id)
        {
            var value = _context.Images.Find(id);
            _context.Images.Remove(value);
            _context.SaveChanges();
            return Ok("Görsel silme işlemi başarılı.");
        }
        [HttpGet("GetImage")]
        public IActionResult GetImage(int id)
        {
            var value = _context.Images.Find(id);
            return Ok(value);
        }
        [HttpPut]
        public IActionResult UptadeImage(UpdateImageDto updateImageDto)
        {
            var value = _mapper.Map<Image>(updateImageDto);
            _context.Images.Update(value);
            _context.SaveChanges();
            return Ok("Görsel güncelleme başarılı");
        }
    }
}
