using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Application.DTOs
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public string Url { get; set; } = string.Empty;
    }
}
