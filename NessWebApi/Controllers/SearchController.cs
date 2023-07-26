using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


        [HttpGet]
        public async Task<IActionResult> SearchUsers (string searching)
        {
            if(string.IsNullOrEmpty(searching))
            {
                var allUsers= await _dbContextNessApp.Users.ToListAsync();
                return Ok(allUsers);
            }
            
            var matchingUsers = await _dbContextNessApp.Users.Where(x => x.Username.Contains(searching)).ToListAsync();

            return Ok(matchingUsers);
        }
    }
}
