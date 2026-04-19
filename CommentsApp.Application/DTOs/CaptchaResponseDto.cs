namespace CommentsApp.Application.DTOs
{
    public class CaptchaResponseDto
    {
        public string CaptchaId { get; set; } = string.Empty;
        public string ImageBase64 { get; set; } = string.Empty;
    }
}
