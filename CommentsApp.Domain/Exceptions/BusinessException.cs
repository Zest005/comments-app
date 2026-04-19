namespace CommentsApp.Domain.Exceptions
{
    public class BusinessException : Exception
    {
        public string FieldName { get; }

        public BusinessException(string message, string fieldName = "general")
            : base(message)
        {
            FieldName = fieldName;
        }
    }

    public class CaptchaValidationException : BusinessException
    {
        public CaptchaValidationException(string message)
            : base(message, "captchaText") { }
    }

    public class HtmlValidationException : BusinessException
    {
        public HtmlValidationException(string message)
            : base(message, "text") { }
    }

    public class FileUploadException : BusinessException
    {
        public FileUploadException(string message)
            : base(message, "general") { }
    }
}
