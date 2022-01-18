using Apollo.BLInterface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : Controller
    {
        private readonly IImageService imageService;

        public ImageController(IImageService imageService)
        {
            this.imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        [HttpGet("{imageName}")]
        public async Task<FileStreamResult> GetImage([FromRouteAttribute] string imageName)
        {
            byte[] data;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                try
                {
                    await imageService.GetImageAsync(imageName, bw);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    try
                    {
                        await imageService.GetDefaultImage(bw);
                    }
                    catch (Exception ex1)
                    {
                        Response.StatusCode = 404;
                        return new FileStreamResult(new MemoryStream(new byte[0]), "image/jpeg");
                    }
                }
                data = ms.ToArray();
            }

            return new FileStreamResult(new MemoryStream(data), "image/jpeg");
        }

        [HttpPost("upload/movieimage/{fileName}")]
        public async Task<ActionResult> UploadImageAsync([FromRoute] string fileName, IFormFile file)
        {
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);

                imageService.StoreImage(fileName, ms.ToArray());
            }
            return Ok();
        }
    }
}
