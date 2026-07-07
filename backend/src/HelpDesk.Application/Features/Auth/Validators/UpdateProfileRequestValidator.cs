using FluentValidation;
using HelpDesk.Application.Features.Auth.Dtos;

namespace HelpDesk.Application.Features.Auth.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Department).MaximumLength(100);
        RuleFor(x => x.JobTitle).MaximumLength(100);
    }
}
