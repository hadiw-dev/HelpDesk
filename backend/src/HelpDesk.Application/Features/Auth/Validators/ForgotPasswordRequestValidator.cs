using FluentValidation;
using HelpDesk.Application.Features.Auth.Dtos;

namespace HelpDesk.Application.Features.Auth.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
