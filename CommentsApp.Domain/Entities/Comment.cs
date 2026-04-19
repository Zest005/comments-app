namespace CommentsApp.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? HomePage { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? ParentCommentId { get; set; }

        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public CommentAttachment? Attachment { get; set; }
    }
}
