using Application.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("")]
    public class FilesController : ControllerBase
    {
        private readonly IUserAccessor _userAccessor;

        public FilesController(IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
        }

        // NO LIMIT for images cause it's not needed, for this is a personal API
        // and not a commercial one.

        [HttpPost("uploadImages")]
        public async Task<IActionResult> UploadImages(List<IFormFile> images)
        {
            try
            {
                List<string> imagesUrls = new();

                //var username = "testing";// _userAccessor.GetUsername();
                var username = _userAccessor.GetUsername();


                foreach (var image in images)
                {
                    var sanitizedFileName = SanitizeFileName(image.FileName);

                    var imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", username);

                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }


                    if (IsImageFile(image))
                    {
                        var imagePath = Path.Combine(imagesDirectory, sanitizedFileName);
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                            // when it adds it successfully, add the image to the url list
                            imagesUrls.Add($"images/{sanitizedFileName}");
                        }
                    }
                }

                return Ok(imagesUrls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            try
            {
                string imageUrl = "";
                var currentPort = HttpContext.Request.Host.Port;

                //var username = "testing";// _userAccessor.GetUsername();
                var username =  _userAccessor.GetUsername();

                if (string.IsNullOrEmpty(username)) return StatusCode(401, "Login first please . . .");

                var sanitizedFileName = SanitizeFileName(image.FileName);

                //var imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", username);
                var imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
                if (!Directory.Exists(imagesDirectory))
                {
                    Directory.CreateDirectory(imagesDirectory);
                }


                if (IsImageFile(image))
                {
                    //var imagePath = Path.Combine(imagesDirectory, sanitizedFileName);
                    var imagePath = Path.Combine(imagesDirectory, $"{username}-{sanitizedFileName}");

                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                        // when it adds it successfully, add the image to the url list
                        //imageUrl = ($"http://localhost:{currentPort}/images/{sanitizedFileName}");
                        imageUrl = ($"http://localhost:{currentPort}/images/{username}-{sanitizedFileName}");

                    }
                }

                if (!imageUrl.IsNullOrEmpty())
                    return new JsonResult(new
                    {
                        success = 1,
                        file = new { url = imageUrl }
                    });
                return StatusCode(500, "couldn't save the image");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        private bool IsImageFile(IFormFile file)
        {
            // Check if the file has a valid image extension (you can add more extensions if needed)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            return file.Length > 0 && allowedExtensions.Contains(fileExtension);
        }


        private string SanitizeFileName(string fileName)
        {
            var sanitizedName = fileName.Replace(" ", "-");

            sanitizedName = sanitizedName.Trim();
            return Path.GetFileName(sanitizedName);
        }

        [AllowAnonymous] // for now, until i learn how to override the request cycle in nextJs
        [HttpGet("images/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            try
            {

                //var username =_userAccessor.GetUsername();

                //var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", username, fileName);
                var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images",  fileName);

                if (System.IO.File.Exists(imagePath))
                {
                    // Determine the content type based on the file extension (you can add more types as needed)
                    var contentType = GetContentType(fileName);

                    return PhysicalFile(imagePath, contentType);
                }
                else
                {
                    return NotFound("Image not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private string GetContentType(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName).ToLower();
            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                default:
                    return "application/octet-stream"; // Default content type
            }
        }


    }

    // not needed . . .  for  now 
    public class Photo
    {
        public Guid Id { get; set; }
        public byte[] Bytes { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileExtension { get; set; }
        public decimal Size { get; set; }
    }
}

