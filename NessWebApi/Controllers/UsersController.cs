using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NessWebApi.Data;
using NessWebApi.Dto;
using NessWebApi.Helper;
using NessWebApi.Models;
using NessWebApi.UtilityService;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
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

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbContextNessApp.Users
                .Include(u => u.FavoriteEvents)
                .ToListAsync();

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
        public async Task<IActionResult> Authenticate([FromBody] UserLoginDto userObj)
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

            var token = CreateJwt(user);
            user.Token = token;
            await _dbContextNessApp.SaveChangesAsync();

            return Ok(new
            {
                Message = "Login Success !",
                Token = token
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


        private Task<bool> UsernameAlreadyExistsAsync(string username)
        {
            return _dbContextNessApp.Users.AnyAsync(x => x.Username == username);
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysecret...");
            var identity = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email),
                });
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

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
                    StatusCode = 404,
                    Message = "Email doesn't exist "
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
                StatusCode = 200,
                Message = "Email Sent !"
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            var user = await _dbContextNessApp.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User Doesn't Exist "
                });
            }
            var tokenCode = user.ResetPasswordToken;
            DateTime emailTokenExpiry = user.ResetPasswordExpiry;
            if (tokenCode != resetPasswordDto.EmailToken || emailTokenExpiry < DateTime.Now)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid Reset link"
                });
            }
            user.Password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            _dbContextNessApp.Entry(user).State = EntityState.Modified;
            await _dbContextNessApp.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Password Reset Successfully"

            });
        }

        [HttpGet("get-favorite-events")]
        [Authorize]
        public async Task<IActionResult> GetFavoriteEvents()
        {
            try
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
                        var favoriteEvents = user.FavoriteEvents.ToList();
                        return Ok(favoriteEvents);
                    }
                    else
                    {
                        return NotFound(new { Message = "Userul nu s-a gasit !" });
                    }
                }
                else
                {
                    return Unauthorized(new { Message = "Unauthorized access." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "A apărut o eroare internă." });
            }
        }


        [HttpPost("image-upload-user/{userId}")]
        public async Task<IActionResult> UploadImageForUser([FromRoute] int userId, [FromForm] FileUpload fileUpload)
        {
            try
            {
                if (fileUpload.files != null && fileUpload.files.Length > 0)
                {
                    string path = _webHostEnvironment.WebRootPath + "\\users-images\\";

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

                    var userSearch = await _dbContextNessApp.Users.FindAsync(userId);
                    if (userSearch != null)
                    {
                        userSearch.ImageUrl = fileName;
                        await _dbContextNessApp.SaveChangesAsync();
                    }
                    else
                    {
                        return NotFound(new { Message = $"User with id {userId} was Not Found !" });
                    }
                    var uploadedFile = new UploadedFile
                    {
                        FileName = fileUpload.files.FileName,
                        UploadDateTime = DateTime.Now,
                        FileSize = fileUpload.files.Length,
                        ImageUrl = "/users-images/" + fileUpload.files.FileName,
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
                return StatusCode(500, "Internal server error !");
            }
        }

        [HttpGet("give-user-image")]
        [Authorize] // Asigurați-vă că utilizatorul este autentificat pentru a accesa această resursă
        public async Task<IActionResult> GetUserImage()
        {  
            // Obțineți ID-ul utilizatorului autentificat
            var emailClaim = User.FindFirst(ClaimTypes.Email);

            if (emailClaim == null)
            {
                return Unauthorized("Utilizatorul nu este autentificat.");
            }

            string userEmail = emailClaim.Value;

            // Aici ar trebui să aveți o metodă sau un mecanism pentru a obține URL-ul imaginii asociate utilizatorului cu ID-ul specificat
            // Presupunem că aveți o entitate User în baza de date cu un câmp ImageUrl care conține URL-ul imaginii utilizatorului

            var user = await _dbContextNessApp.Users.FirstOrDefaultAsync(user=>user.Email==userEmail);

            if (user != null && !string.IsNullOrEmpty(user.ImageUrl))
            {
                // Construiți calea completă către imagine
                string path = _webHostEnvironment.WebRootPath + "\\users-images\\";
                string imageUrl = user.ImageUrl;

                // Combinați URL-ul imaginii cu extensia din ruta cererii
                string fullImageUrl = Path.Combine(path, imageUrl);

                if (System.IO.File.Exists(fullImageUrl))
                {
                    byte[] imageBytes = System.IO.File.ReadAllBytes(fullImageUrl);

                    // Returnați imaginea cu tipul de conținut corect
                    return File(imageBytes, "image/jpg"); // Modificați tipul de conținut la tipul adecvat imaginilor utilizate
                }
            }

            return NotFound("Imaginea utilizatorului nu a fost găsită.");
        }

    }
}
