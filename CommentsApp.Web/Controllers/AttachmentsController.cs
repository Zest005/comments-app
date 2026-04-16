using CommentsApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommentsApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttachmentsController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly string _uploadsPath;

        public AttachmentsController(ICommentRepository commentRepository, IWebHostEnvironment environment)
        {
            _commentRepository = commentRepository;
            _uploadsPath = Path.Combine(environment.ContentRootPath, "uploads");
        }

        [HttpGet("{commentId:int}")]
        public async Task<ActionResult> GetAttachments(int commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment?.Attachment == null)
                return NotFound(new { Error = "Attachment not found." });

            var filePath = Path.Combine(_uploadsPath, comment.Attachment.StoredFilePath);
            
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Error = "File not found on server." });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(fileBytes, comment.Attachment.ContentType, comment.Attachment.FileName);
        }
    }
}
