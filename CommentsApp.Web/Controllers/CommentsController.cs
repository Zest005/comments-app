using CommentsApp.Application.Common.Validators;
using CommentsApp.Application.DTOs;
using CommentsApp.Application.Services;
using CommentsApp.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CommentsApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly CreateCommentValidator _validator;
        private readonly IHubContext<CommentHub> _hubContext;

        public CommentsController(CommentService commentService, CreateCommentValidator validator, IHubContext<CommentHub> hubContext)
        {
            _commentService = commentService;
            _validator = validator;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<CommentDto>>> GetComments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] bool sortDescending = true)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1 || pageSize > 25)
                pageSize = 25;

            var result = await _commentService.GetCommentsAsync(page, pageSize, sortBy, sortDescending);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(
            [FromForm] CreateCommentDto dto,
            [FromForm] IFormFile? file = null)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return BadRequest(new { Errors = errors });
            }

            byte[]? fileData = null;
            string? fileName = null;
            string? fileContentType = null;

            if (file != null && file.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
                fileName = file.FileName;
                fileContentType = file.ContentType;
            }

            try
            {
                var comment = await _commentService.CreateCommentAsync(dto, fileData, fileName, fileContentType);

                await _hubContext.Clients.All.SendAsync("NewComment", comment);

                return CreatedAtAction(nameof(GetComments), new { id = comment.Id }, comment);
            }
            catch (InvalidOperationException ex)
            {
                string fieldName;
                if (ex.Message.Contains("CAPTCHA", StringComparison.OrdinalIgnoreCase))
                    fieldName = "captchaText";
                else if (ex.Message.Contains("tag", StringComparison.OrdinalIgnoreCase))
                    fieldName = "text";
                else
                    fieldName = "general";

                return BadRequest(new
                {
                    Errors = new Dictionary<string, string[]>
                    {
                        { fieldName, new[] { ex.Message } }
                    }
                });
            }
        }
    }
}
