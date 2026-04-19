using CommentsApp.Application.Common.Interfaces;
using CommentsApp.Application.DTOs;
using CommentsApp.Domain.Entities;
using CommentsApp.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace CommentsApp.Application.Services
{
    public class CommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICaptchaService _captchaService;
        private readonly IFileService _fileService;

        private static readonly HashSet<string> AllowedTags = new() { "a", "code", "i", "strong" };

        public CommentService(ICommentRepository commentRepository, ICaptchaService captchaService, IFileService fileService)
        {
            _commentRepository = commentRepository;
            _captchaService = captchaService;
            _fileService = fileService;
        }

        public async Task<PagedResultDto<CommentDto>> GetCommentsAsync(int page, int pageSize, string sortBy, bool sortDescending)
        {
            var (comments, totalCount) = await _commentRepository.GetRootCommentsAsync(page, pageSize, sortBy, sortDescending);
            
            return new PagedResultDto<CommentDto>
            {
                Items = comments.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CommentDto> CreateCommentAsync(
            CreateCommentDto dto,
            byte[]? fileData = null,
            string? fileName = null,
            string? fileContentType = null)
        {
            ValidateCaptchaIfNeeded(dto, fileData != null && fileName != null);
            ValidateHtmlTags(dto.Text);

            if (dto.ParentCommentId.HasValue)
            {
                var parentExists = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value);
                if (parentExists == null)
                    throw new BusinessException("Parent comment not found.", "general");
            }

            var comment = new Comment
            {
                UserName = dto.UserName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                HomePage = string.IsNullOrWhiteSpace(dto.HomePage) ? null : dto.HomePage.Trim(),
                Text = SanitizeHtml(dto.Text),
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = dto.ParentCommentId
            };

            if (fileData != null && fileName != null && fileContentType != null)
                comment.Attachment = await ProcessFileAsync(fileData, fileName, fileContentType);

            var savedComment = await _commentRepository.AddAsync(comment);
            return MapToDto(savedComment);
        }

        private void ValidateCaptchaIfNeeded(CreateCommentDto dto, bool hasFile)
        {
            if (!hasFile) return;

            if (string.IsNullOrWhiteSpace(dto.CaptchaId) || string.IsNullOrWhiteSpace(dto.CaptchaText))
                throw new CaptchaValidationException("CAPTCHA is required when uploading a file.");

            if (!_captchaService.ValidateCaptcha(dto.CaptchaId, dto.CaptchaText))
                throw new CaptchaValidationException("Invalid CAPTCHA.");
        }

        private static void ValidateHtmlTags(string input)
        {
            var tagStack = new Stack<string>();
            var tagMatches = Regex.Matches(input, @"<(/?)(\w+)([^>]*)>");

            foreach (Match match in tagMatches)
            {
                var isClosing = match.Groups[1].Value == "/";
                var tagName = match.Groups[2].Value.ToLower();

                if (!AllowedTags.Contains(tagName))
                    continue;

                if (isClosing)
                {
                    if (tagStack.Count == 0 || tagStack.Peek() != tagName)
                        throw new HtmlValidationException(
                            $"Unexpected closing tag </{tagName}>. Tags must be properly nested.");
                    tagStack.Pop();
                }
                else
                    tagStack.Push(tagName);
            }

            if (tagStack.Count > 0)
            {
                var unclosed = string.Join(", ", tagStack.Select(t => $"<{t}>"));
                throw new HtmlValidationException(
                    $"Unclosed tags detected: {unclosed}. All tags must be properly closed.");
            }
        }

        private async Task<CommentAttachment> ProcessFileAsync(
            byte[] fileData, string fileName, string fileContentType)
        {
            var uploadResult = await _fileService.SaveFileAsync(fileData, fileName, fileContentType);

            if (!uploadResult.Success)
                throw new FileUploadException(uploadResult.ErrorMessage ?? "File upload failed.");

            return new CommentAttachment
            {
                FileName = fileName,
                StoredFilePath = uploadResult.StoredFilePath,
                ContentType = fileContentType,
                FileSize = uploadResult.FileSize
            };
        }

        public async Task<PagedResultDto<CommentDto>> GetRepliesAsync(int parentId, int skip, int take)
        {
            var (replies, totalCount) = await _commentRepository.GetRepliesAsync(parentId, skip, take);

            return new PagedResultDto<CommentDto>
            {
                Items = replies.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = skip / take + 1,
                PageSize = take
            };
        }

        private CommentDto MapToDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                UserName = comment.UserName,
                Email = comment.Email,
                HomePage = comment.HomePage,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                ParentCommentId = comment.ParentCommentId,
                Attachment = comment.Attachment != null ? new AttachmentDto
                {
                    Id = comment.Attachment.Id,
                    FileName = comment.Attachment.FileName,
                    ContentType = comment.Attachment.ContentType,
                    FileSize = comment.Attachment.FileSize,
                    Url = $"/api/attachments/{comment.Id}"
                } : null,
                Replies = new List<CommentDto>(),
                ReplyCount = comment.Replies?.Count ?? 0
            };
        }

        private static string SanitizeHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var result = Regex.Replace(input, @"<(/?)(\w+)([^>]*)>", match =>
            {
                var isClosing = match.Groups[1].Value == "/";
                var tagName = match.Groups[2].Value.ToLower();

                if (!AllowedTags.Contains(tagName))
                    return match.Value.Replace("<", "&lt;").Replace(">", "&gt;");

                if (isClosing)
                    return $"</{tagName}>";

                if (tagName == "a")
                {
                    var attributes = match.Groups[3].Value;
                    var href = Regex.Match(attributes, @"href\s*=\s*""([^""]*)""|href\s*=\s*'([^']*)'");
                    var title = Regex.Match(attributes, @"title\s*=\s*""([^""]*)""|title\s*=\s*'([^']*)'");

                    var cleanTag = "<a";
                    if (href.Success)
                    {
                        var hrefValue = href.Groups[1].Success ? href.Groups[1].Value : href.Groups[2].Value;

                        if (!hrefValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                            !hrefValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            hrefValue = "#";
                        }

                        hrefValue = System.Net.WebUtility.HtmlEncode(hrefValue);
                        cleanTag += $@" href=""{hrefValue}""";
                    }
                    if (title.Success)
                    {
                        var titleValue = title.Groups[1].Success ? title.Groups[1].Value : title.Groups[2].Value;
                        titleValue = System.Net.WebUtility.HtmlEncode(titleValue);
                        cleanTag += $@" title=""{titleValue}""";
                    }
                    cleanTag += @" target=""_blank"" rel=""noopener noreferrer"">";
                    return cleanTag;
                }

                return $"<{tagName}>";
            });

            return result.Replace("\r\n", "<br>").Replace("\n", "<br>");
        }
    }
}
