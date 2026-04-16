using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        // валидация
        public string Email { get; set; } = string.Empty;
        // валидация URL
        public string? HomePage { get; set; }
        // защита от XSS
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // null - родительский коммент, !null - ответ
        public int? ParentCommentId { get; set; }

        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public CommentAttachment? Attachment { get; set; }
    }
}
