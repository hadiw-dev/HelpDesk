using FluentValidation;
using HelpDesk.Application.Features.Comments.Dtos;

namespace HelpDesk.Application.Features.Comments.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
    }
}
