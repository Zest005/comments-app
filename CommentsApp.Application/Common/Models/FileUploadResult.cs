namespace CommentsApp.Application.Common.Models
{
    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string StoredFilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
