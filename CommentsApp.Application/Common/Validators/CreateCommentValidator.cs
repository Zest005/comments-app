using CommentsApp.Application.DTOs;
using FluentValidation;

namespace CommentsApp.Application.Common.Validators
{
    public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name is required.")
                .MaximumLength(30).WithMessage("User name cannot exceed 30 characters.")
                .Matches(@"^[a-zA-Z0-9]+$").WithMessage("User Name must contain only Latin letters and digits.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(255).WithMessage("E-mail must not exceed 254 characters.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.HomePage)
                .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    .WithMessage("Home page must be a valid URL.");

            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Comment text is required.");
        }
    }
}
