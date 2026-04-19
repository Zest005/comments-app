namespace CommentsApp.Domain.Entities
{
    public class CommentAttachment
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public Comment Comment { get; set; } = null!;
    }
}
