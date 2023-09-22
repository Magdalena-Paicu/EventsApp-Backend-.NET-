
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NessWebApi.Data;
using NessWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        [HttpPost("load-image/{EventId}")]
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


        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateEvent([FromRoute]int id, Event updateEvent, IFormFile file)
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
            ev.withTicket = updateEvent.withTicket;

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

        [HttpPatch]
        public async Task<IActionResult> ChangeTitleForEvent(int eventId, string titleChanged)
        {
            var ev = _dbContextNessApp.Events.FirstOrDefault(ev => ev.Id == eventId);
            if (ev != null)
            {
                ev.Title = titleChanged;
                await _dbContextNessApp.SaveChangesAsync();
                return Ok(new { Message ="The title changed successfully !"});
            };
            return NotFound();
        }

        [HttpPost("add-event/{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> AddEventToUser(int eventId)
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email);

            if (emailClaim != null)
            {
                var userEmail = emailClaim.Value;

                var user = await _dbContextNessApp.Users
                    .Include(u => u.FavoriteEvents)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);

                var ev = await _dbContextNessApp.Events.FindAsync(eventId);

                if (user == null || ev == null)
                {
                    return NotFound(new { Message = "User or Event Not Found!" });
                }
                if (user.FavoriteEvents.Any(e => e.Id == eventId))
                {
                    return BadRequest(new { Message = "Event is already added to the user!" });
                }

                user.FavoriteEvents.Add(ev);
                await _dbContextNessApp.SaveChangesAsync();

                return Ok(new { Message = "Event added to user successfully!", Event = ev });
            }
            else
            {
                return NotFound(new { Message = "Email claim not found." });
            }
        }

        [HttpDelete("delete-event/{eventId:int}")]
        [Authorize]

        public async Task<IActionResult> DeleteEventForUser(int eventId)
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email);

            if (emailClaim != null)
            {
                var userEmail = emailClaim.Value;

                var user = await _dbContextNessApp.Users
                   .Include(u => u.FavoriteEvents)
                   .FirstOrDefaultAsync(u => u.Email == userEmail);

                var ev = await _dbContextNessApp.Events.FindAsync(eventId);

                if (user == null || ev == null)
                {
                    return NotFound(new { Message = "User or Event Not Found !" });
                }

                if (!(user.FavoriteEvents.Any(ev => ev.Id == eventId))){
                    return BadRequest(new { Message = "Event is not in Favorite List !" });
                }
                user.FavoriteEvents.Remove(ev);
                await _dbContextNessApp.SaveChangesAsync();

                return Ok(new { Message = "Event deleted successfully !" });
            }
            else
            {
                return NotFound(new { Message = "Email claim not found ." });
            }

        }

        [HttpGet("is-element-favorite")]
        public async Task<bool> IsEventsFavorite(int eventId)
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                var userEmail = emailClaim.Value;

                var user = await _dbContextNessApp.Users
                 .Include(u => u.FavoriteEvents)
                 .FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user != null)
                {
                    return user.FavoriteEvents.Any(ev => ev.Id == eventId);

                }
            }
            return false;
        }

    }
}