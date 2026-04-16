using CommentsApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommentsApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaptchaController : ControllerBase
    {
        private readonly ICaptchaService _captchaService;

        public CaptchaController(ICaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        [HttpGet]
        public ActionResult GenerateCaptcha()
        {
            var (captchaId, imageBytes) = _captchaService.GenerateCaptcha();

            var base64Image = Convert.ToBase64String(imageBytes);

            return Ok(new { CaptchaId = captchaId, ImageBase64 = $"data:image/png;base64,{base64Image}" });
        }
    }
}
