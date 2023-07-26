using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NessWebApi.Data;
using NessWebApi.Models;

namespace NessWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly DbContextNessApp _dbContextNessApp;

        public EventsController(IWebHostEnvironment webHostEnvironment, DbContextNessApp dbContextNessApp)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContextNessApp = dbContextNessApp;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var allEvents = await _dbContextNessApp.Events.ToListAsync();
            return Ok(allEvents);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _dbContextNessApp.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound();
            }

            return Ok(ev);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewEvent([FromForm] Event newEvent, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }


                if (!_dbContextNessApp.Events.Any())
                {
                    newEvent.Id = 0;
                }
                else
                {
                    int maxExistingId = _dbContextNessApp.Events.Max(e => e.Id);
                    newEvent.Id = maxExistingId + 1;
                }

                newEvent.ImageUrl = "/uploads/" + uniqueFileName;

                _dbContextNessApp.Events.Add(newEvent);
                await _dbContextNessApp.SaveChangesAsync();

                return Ok(newEvent);
            }

            return BadRequest("No file or file is empty.");
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] Event updateEvent, IFormFile file)
        {
            var ev = await _dbContextNessApp.Events.FindAsync(id);
            if (ev == null)
            {
                return NotFound();
            }

            ev.Title = updateEvent.Title;
            // Actualizează restul proprietăților evenimentului

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                ev.ImageUrl = "/uploads/" + uniqueFileName;
            }

            await _dbContextNessApp.SaveChangesAsync();

            return Ok(ev);
        }

        [HttpDelete("{id:int}")]

        public async Task<IActionResult> DeleteEvent(int id) {
            var ev = await _dbContextNessApp.Events.FindAsync(id);
            if (ev != null)
            {
                _dbContextNessApp.Events.Remove(ev);
                await _dbContextNessApp.SaveChangesAsync();
                return Ok(ev);
            }

            return NotFound();
        }
    }
}