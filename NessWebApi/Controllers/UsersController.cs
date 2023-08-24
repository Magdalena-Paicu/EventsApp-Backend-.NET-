using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NessWebApi.Data;
using NessWebApi.Models;

namespace NessWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly DbContextNessApp _dbContextNessApp;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(DbContextNessApp dbContextNessApp, IWebHostEnvironment webHostEnvironment)
        {
            _dbContextNessApp = dbContextNessApp;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbContextNessApp.Users.ToListAsync();

            return Ok(users);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _dbContextNessApp.Users.FindAsync(id);

            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, User updateUser)
        {

            var userFind = await _dbContextNessApp.Users.FindAsync(id);

            if (updateUser != null)
            {

                userFind.Email = updateUser.Email;
                userFind.Username = updateUser.Username;
                userFind.Password = updateUser.Password;
                userFind.Token = updateUser.Token;
                userFind.Role = updateUser.Role;

                await _dbContextNessApp.SaveChangesAsync();

                return Ok(updateUser);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {

            var user = await _dbContextNessApp.Users.FindAsync(id);
            if (user != null)
            {
                _dbContextNessApp.Remove(user);
                await _dbContextNessApp.SaveChangesAsync();
                return Ok(user);
            }
            return NotFound();
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _dbContextNessApp.Users.FirstOrDefaultAsync(x => x.Email == userObj.Email && x.Password == userObj.Password);
            if (user == null)
                return NotFound(new { Message = " User Not Found !" });
            return Ok(new { Message = "Login Success !" });
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if(userObj == null)
                return BadRequest();    

            await _dbContextNessApp.Users.AddAsync(userObj);
            await _dbContextNessApp.SaveChangesAsync();
            return Ok(new { Message = "User Register !", userObj });
        }
    }
}
