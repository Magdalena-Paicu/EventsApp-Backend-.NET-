using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NessWebApi.Data;
using NessWebApi.Models;

namespace NessWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly DbContextNessApp _dbContextNessApp;

        public SearchController(DbContextNessApp dbContextNessApp)
        {
            _dbContextNessApp = dbContextNessApp;
        }

        [HttpGet("searchByTitle")]
        public async Task<IActionResult> SearchEventsByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                var allEvents = await _dbContextNessApp.Events.ToListAsync();
                return Ok(allEvents);
            }
            else
            {
                var matchingEvents = await _dbContextNessApp.Events.Where(x => x.Title.Contains(title)).ToListAsync();
                return Ok(matchingEvents);
            }
        }

        [HttpGet("searchIsFree")]
        public async Task<IActionResult> SearchEventsFree([FromQuery] bool isFree)
        {
            try
            {
                var allEventsFree = await _dbContextNessApp.Events.Where(x => x.isFree == isFree).ToListAsync();
                if (allEventsFree.Any())
                {
                    return Ok(allEventsFree);
                }
                else
                {
                    return NotFound(" Nu s-au gasit elementele corespunzatoare !");
                }
            }
            catch (Exception exception)
            {
                return StatusCode(500, "A aparut o eroare in timpul cautarii evenimentelor.");
            }
        }

        [HttpGet("searchWithTicket")]
        public async Task<IActionResult> SearchEventsWithTicket([FromQuery] bool withTicket)
        {
            try
            {
                var matchingEvents = await _dbContextNessApp.Events.Where(x => x.withTicket == withTicket).ToListAsync();
                if (matchingEvents.Any())
                {
                    return Ok(matchingEvents);
                }
                else { return NotFound(" Nu s-au gasit elementele corespunzatoare !"); }

            }
            catch (Exception exception)
            {
                return StatusCode(500, "Aaparut o eroare in timpul cautarii evenimentelor. ");
            }
        }
    }
}
