
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NessWebApi.Data;
using NessWebApi.Models;

namespace NessWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadsController : Controller
    {
        public static IWebHostEnvironment _webHostEnvironment;
        private readonly DbContextNessApp _dbContextNessApp;


        public FileUploadsController(IWebHostEnvironment webHostEnvironment, [FromServices] DbContextNessApp dbContextNessApp)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContextNessApp = dbContextNessApp;
        }

        [HttpPost]
        public async Task<string> Post([FromForm] FileUpload fileUpload)
        {
            try
            {
                if (fileUpload.files != null && fileUpload.files.Length > 0)
                {
                    string path = _webHostEnvironment.WebRootPath + "\\uploads\\";

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using (FileStream fileStream = System.IO.File.Create(path + fileUpload.files.FileName))
                    {
                        fileUpload.files.CopyTo(fileStream);
                        fileStream.Flush();
                    }

                    var uploadedFile = new UploadedFile
                    {
                        FileName = fileUpload.files.FileName,
                        UploadDateTime = DateTime.Now,
                        FileSize = fileUpload.files.Length,
                        ImageUrl = "/uploads/" + fileUpload.files.FileName
                    };

                    _dbContextNessApp.UploadedFiles.Add(uploadedFile);
                    await _dbContextNessApp.SaveChangesAsync();


                    return "Upload Done.";
                }
                else
                {
                    return "Failed!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //[HttpGet("{fileName}")]
        //public async Task<IActionResult> Get([FromRoute] string fileName)
        //{
        //    string path = _webHostEnvironment.WebRootPath + "\\events-images\\";
        //    var filePathPng = path + fileName + ".png";
        //    var filePathJpg = path + fileName + ".jpg";

        //    if (System.IO.File.Exists(filePathPng))
        //    {
        //        byte[] b = System.IO.File.ReadAllBytes(filePathPng);
        //        return File(b, "image/png");
        //    }

        //    if (System.IO.File.Exists(filePathJpg))
        //    {
        //        byte[] b = System.IO.File.ReadAllBytes(filePathJpg);
        //        return File(b, "image/jpg");
        //    }

        //    return NotFound("Fișierul nu a fost găsit.");
        //}

        [HttpGet("give-event-image/{fileNameWithExtension}")]
        public async Task<IActionResult> GetEventImage([FromRoute] string fileNameWithExtension)
        {
            // Extrageți numele fișierului și extensia
            string fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
            string extension = Path.GetExtension(fileNameWithExtension).ToLower();

            string path = _webHostEnvironment.WebRootPath + "\\events-images\\";
            string filePath = path + fileNameWithExtension;

            if (System.IO.File.Exists(filePath))
            {
                byte[] b = System.IO.File.ReadAllBytes(filePath);
                // Construiți URL-ul cu numele fișierului și extensia corespunzătoare
                string imageUrl = "/events-images/" + fileNameWithExtension;
                return File(b, "image/" + extension);
            }

            return NotFound("Fișierul nu a fost găsit.");
        }


        [HttpGet("give-user-image/{fileNameWithExtension}")]
        public async Task<IActionResult> GetUserImage([FromRoute] string fileNameWithExtension)
        {
            // Extrageți numele fișierului și extensia
            string fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
            string extension = Path.GetExtension(fileNameWithExtension).ToLower();

            string path = _webHostEnvironment.WebRootPath + "\\users-images\\";
            string filePath = path + fileNameWithExtension;

            if (System.IO.File.Exists(filePath))
            {
                byte[] b = System.IO.File.ReadAllBytes(filePath);
                // Construiți URL-ul cu numele fișierului și extensia corespunzătoare
                string imageUrl = "/users-images/" + fileNameWithExtension;
                return File(b, "image/" + extension);
            }

            return NotFound("Fișierul nu a fost găsit.");
        }


        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            string path = _webHostEnvironment.WebRootPath + "\\uploads\\";

            try
            {
                var uploadedFile = await _dbContextNessApp.UploadedFiles.FirstOrDefaultAsync(file => file.FileName == fileName + ".jpg" || file.FileName == fileName + ".png");

                if (uploadedFile != null)
                {
                    _dbContextNessApp.UploadedFiles.Remove(uploadedFile);
                    await _dbContextNessApp.SaveChangesAsync();

                    if (System.IO.File.Exists(path + fileName + ".jpg"))
                    {
                        System.IO.File.Delete(path + fileName + ".jpg");

                        return Ok("File deleted successfully.");
                    }
                    else if (System.IO.File.Exists(path + fileName + ".png"))
                    {
                        System.IO.File.Delete(path + fileName + ".png");
                        return Ok("File deleted successfully.");
                    }
                }
                else
                {
                    return NotFound("The file was not found in the database.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return NotFound("Fișierul nu a fost găsit pe disc.");
        }


        [HttpGet]
        public async Task<IActionResult> GetAllImages()
        {
            try
            {
                var uploadedFiles = await _dbContextNessApp.UploadedFiles.ToListAsync();

                return Ok(uploadedFiles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }


}
