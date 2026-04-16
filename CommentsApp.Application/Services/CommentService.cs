using CommentsApp.Application.Common.Interfaces;
using CommentsApp.Application.DTOs;
using CommentsApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Application.Services
{
    public class CommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICaptchaService _captchaService;
        private readonly IFileService _fileService;

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
            var isCaptchaValid = _captchaService.ValidateCaptcha(dto.CaptchaId, dto.CaptchaText);
            if (!isCaptchaValid)
                throw new InvalidOperationException("Invalid CAPTCHA");

            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value);

                if (parentComment == null)
                    throw new InvalidOperationException("Parent comment not found.");

                if (parentComment.ParentCommentId != null)
                    throw new InvalidOperationException("You can only reply to root comments.");
            }

            var sanitizedText = SanitizeHtml(dto.Text);

            var comment = new Comment
            {
                UserName = dto.UserName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                HomePage = string.IsNullOrWhiteSpace(dto.HomePage) ? null : dto.HomePage.Trim(),
                Text = sanitizedText,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = dto.ParentCommentId
            };

            if (fileData != null && fileName != null && fileContentType != null)
            {
                var uploadResult = await _fileService.SaveFileAsync(fileData, fileName, fileContentType);
                
                if (!uploadResult.Success)
                    throw new InvalidOperationException($"File upload failed: {uploadResult.ErrorMessage}");

                comment.Attachment = new CommentAttachment
                {
                    FileName = fileName,
                    StoredFilePath = uploadResult.StoredFilePath,
                    ContentType = fileContentType,
                    FileSize = uploadResult.FileSize
                };
            }

            var savedComment = await _commentRepository.AddAsync(comment);

            return MapToDto(savedComment);
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
                }
                : null,
                Replies = comment.Replies
                    .OrderBy(r => r.CreatedAt)
                    .Select(MapToDto)
                    .ToList()
            };
        }

        private string SanitizeHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var allowedTags = new HashSet<string> { "a", "code", "i", "strong" };

            var result = System.Text.RegularExpressions.Regex.Replace(
                input,
                @"<(/?)(\w+)([^>]*)>",
                match =>
                {
                    var tagName = match.Groups[2].Value.ToLower();

                    if (!allowedTags.Contains(tagName))
                    {
                        return match.Value
                            .Replace("<", "&lt;")
                            .Replace(">", "&gt;");
                    }

                    if (tagName == "a" && !match.Groups[1].Value.Contains("/"))
                    {
                        var attributes = match.Groups[3].Value;
                        var href = System.Text.RegularExpressions.Regex.Match(
                            attributes, @"href\s*=\s*""([^""]*)""|href\s*=\s*'([^']*)'");
                        var title = System.Text.RegularExpressions.Regex.Match(
                            attributes, @"title\s*=\s*""([^""]*)""|title\s*=\s*'([^']*)'");

                        var cleanTag = "<a";
                        if (href.Success)
                        {
                            var hrefValue = href.Groups[1].Success
                                ? href.Groups[1].Value
                                : href.Groups[2].Value;
                            cleanTag += $@" href=""{hrefValue}""";
                        }
                        if (title.Success)
                        {
                            var titleValue = title.Groups[1].Success
                                ? title.Groups[1].Value
                                : title.Groups[2].Value;
                            cleanTag += $@" title=""{titleValue}""";
                        }

                        cleanTag += @" target=""_blank"" rel=""noopener noreferrer""";
                        cleanTag += ">";

                        return cleanTag;
                    }

                    return match.Value;
                });

            result = result.Replace("\r\n", "<br>").Replace("\n", "<br>");

            return result;
        }
    }
}
