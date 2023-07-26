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

        public UsersController(DbContextNessApp dbContextNessApp)
        {
            _dbContextNessApp = dbContextNessApp;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbContextNessApp.Users.ToListAsync();

            return Ok(users);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
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

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User userRequest)
        {
            userRequest.Id = Guid.NewGuid();
            await _dbContextNessApp.Users.AddAsync(userRequest);
            await _dbContextNessApp.SaveChangesAsync();
            return Ok(userRequest);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, User updateUser)
        {

            var userFind = await _dbContextNessApp.Users.FindAsync(id);

            if (updateUser != null)
            {

                userFind.Email = updateUser.Email;
                userFind.Username = updateUser.Username;
                userFind.Password = updateUser.Password;
                userFind.IsAdmin = updateUser.IsAdmin;
                userFind.IsConfirmed = updateUser.IsConfirmed;

                await _dbContextNessApp.SaveChangesAsync();

                return Ok(updateUser);
            }
            else
            {

                return NotFound();
            }

        }


        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
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
    }
}
