using CommentsApp.Application.Common.Interfaces;
using CommentsApp.Application.DTOs;
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

            return Ok(new CaptchaResponseDto
            {
                CaptchaId = captchaId,
                ImageBase64 = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}"
            });
        }
    }
}
