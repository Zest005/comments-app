using CommentsApp.Application.Common.Validators;
using CommentsApp.Application.DTOs;
using CommentsApp.Application.Services;
using CommentsApp.Domain.Exceptions;
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

        [HttpGet("{parentId:int}/replies")]
        public async Task<ActionResult<PagedResultDto<CommentDto>>> GetReplies(
            int parentId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 3)
        {
            if (skip < 0)
                skip = 0;
            if (take < 1 || take > 25)
                take = 3;

            var result = await _commentService.GetRepliesAsync(parentId, skip, take);
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

                if (comment.ParentCommentId == null)
                    await _hubContext.Clients.All.SendAsync("NewComment", comment);
                else
                {
                    await _hubContext.Clients.All.SendAsync("NewReply", new
                    {
                        ParentCommentId = comment.ParentCommentId,
                        ReplyId = comment.Id
                    });
                }

                return CreatedAtAction(nameof(GetComments), new { id = comment.Id }, comment);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new
                {
                    Errors = new Dictionary<string, string[]>
                    {
                        { ex.FieldName, new[] { ex.Message } }
                    }
                });
            }
        }
    }
}
