
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NessWebApi.Data;
using NessWebApi.Models;
using Microsoft.AspNetCore.Authorization;


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
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            await _dbContextNessApp.Events.AddAsync(newEvent);
            await _dbContextNessApp.SaveChangesAsync();
            return Ok(newEvent);
        }

        [HttpPost("images/{EventId}")]
        public async Task<IActionResult> UploadImageForEvent([FromRoute] int EventId, [FromForm] FileUpload fileUpload)
        {
            try
            {
                if (fileUpload.files != null && fileUpload.files.Length > 0)
                {
                    string path = _webHostEnvironment.WebRootPath + "\\events-images\\";

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string fileName = fileUpload.files.FileName;
                    string extension = Path.GetExtension(fileUpload.files.FileName);
                    string fullPath = Path.Combine(path, fileName + extension);

                    using (FileStream fileStream = System.IO.File.Create(path + fileUpload.files.FileName))
                    {
                        fileUpload.files.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                    var eventSearch = await _dbContextNessApp.Events.FindAsync(EventId);
                    if (eventSearch != null)
                    {
                        eventSearch.ImageUrl = fileName;
                        await _dbContextNessApp.SaveChangesAsync();
                    }
                    else
                    {
                        return NotFound($"Event with ID {EventId} not found.");

                    }
                    var uploadedFile = new UploadedFile
                    {
                        FileName = fileUpload.files.FileName,
                        UploadDateTime = DateTime.Now,
                        FileSize = fileUpload.files.Length,
                        ImageUrl = "/events-images/" + fileUpload.files.FileName
                    };
                    _dbContextNessApp.UploadedFiles.Add(uploadedFile);
                    await _dbContextNessApp.SaveChangesAsync();
                    return Ok("Upload Done !");
                }
                else
                {
                    return BadRequest("Image was not upload successfully!");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
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
            ev.Address = updateEvent.Address;
            ev.isPetFriendly = updateEvent.isPetFriendly;
            ev.isKidFriendly = updateEvent.isKidFriendly;
            ev.DurationHours = updateEvent.DurationHours;
            ev.Author = updateEvent.Author;
            ev.createdBy = updateEvent.createdBy;
            ev.EndDateTime = updateEvent.EndDateTime;
            ev.ImageUrl = updateEvent.ImageUrl;
            ev.eventLink = updateEvent.eventLink;
            ev.ticketLink = updateEvent.ticketLink;
            ev.ImageUrl = updateEvent.ImageUrl;
            ev.isDraft = updateEvent.isDraft;
            ev.StartDateTime = updateEvent.StartDateTime;
            ev.Location = updateEvent.Location;

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

        public async Task<IActionResult> DeleteEvent(int id)
        {
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