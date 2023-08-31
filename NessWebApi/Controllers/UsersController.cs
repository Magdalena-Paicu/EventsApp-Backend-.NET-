using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NessWebApi.Data;
using NessWebApi.Dto;
using NessWebApi.Helper;
using NessWebApi.Models;
using NessWebApi.UtilityService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NessWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly DbContextNessApp _dbContextNessApp;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailService _emailService;

        public UsersController(DbContextNessApp dbContextNessApp, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IEmailService emailService)
        {
            _dbContextNessApp = dbContextNessApp;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _emailService = emailService;
        }

        [Authorize]
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

            var user = await _dbContextNessApp.Users.FirstOrDefaultAsync(x => x.Email == userObj.Email);
            if (user == null)
                return NotFound(new { Message = " User Not Found !" });

            if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
            {
                return BadRequest(new { Message = "Incorrect Password" });
            }
            user.Token = CreateJwt(user);

            return Ok(new { Message = "Login Success !" ,
            Token = user.Token
            });
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();
            if (await EmailAlreadyExistsAsync(userObj.Email)) return BadRequest(new { Message = "Email already exists !" });
            if (await UsernameAlreadyExistsAsync(userObj.Username)) return BadRequest(new { Message = "Username already exists !" });
            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = "User";
            userObj.Token = "";
            await _dbContextNessApp.Users.AddAsync(userObj);
            await _dbContextNessApp.SaveChangesAsync();
            return Ok(new { Message = "User Register !", userObj });
        }

        private Task<bool> EmailAlreadyExistsAsync(string email) 
          => _dbContextNessApp.Users.AnyAsync(x => x.Email == email);
       

        private  Task<bool> UsernameAlreadyExistsAsync(string username)
        {
            return _dbContextNessApp.Users.AnyAsync(x => x.Username == username);
        }

        //private static string CheckPasswordStrength() {     // aici pot sa pun validare si pe backend ca sa nu ii dau fiece parola, ca si la frontend, fac validari pe AMBELE
        //}

        private string CreateJwt(User user)
        { 
         var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysecret...");
            var identity = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email)
                });
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms .HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
               Subject = identity,
               Expires = DateTime.Now.AddDays(1),
               SigningCredentials = credentials,
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
        [HttpPost("send-email/{email}")]
        public async Task<IActionResult> SendEmail(string email)
        {
            var user = await _dbContextNessApp.Users.FirstOrDefaultAsync(a => a.Email == email);
            if (user == null)
            {
                return NotFound(new
                {
                  StatusCode=404,
                  Message ="email Doesn't Exist "
                });
            }
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            user.ResetPasswordToken = emailToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            string from = _configuration["EmailSettings:From"];
            var emailModel = new EmailModel(email, "ResetPassword!!", EmailBody.EmailStringBody(email, emailToken));
            _emailService.SendEmail(emailModel);
            _dbContextNessApp.Entry(user).State = EntityState.Modified;
            await _dbContextNessApp.SaveChangesAsync();
            return Ok(new
            {
                StatusCode=200,
                Message ="Email Sent !"
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            var user = await _dbContextNessApp.Users.AsNoTracking().FirstOrDefaultAsync(a=>a.Email ==resetPasswordDto.Email);
            {
                return NotFound(new
                {
                    StatusCode =404,
                    Message = "User Doesn't Exist "
                });
            }
            var tokenCode = user.ResetPasswordToken;
            DateTime emailTokenExpiry = user.ResetPasswordExpiry;
            if(tokenCode !=resetPasswordDto.EmailToken || emailTokenExpiry<DateTime.Now)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message ="Invalid Reset link"
                });
            }

            user.Password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            _dbContextNessApp.Entry(user).State =EntityState.Modified;
            await _dbContextNessApp.SaveChangesAsync();
            return Ok(new
            {
                StatusCode =200,
                Message = "Password Reset Successfully"

            });
        }
    }
}
