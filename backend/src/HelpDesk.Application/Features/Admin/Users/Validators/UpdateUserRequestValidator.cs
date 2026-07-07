using FluentValidation;
using HelpDesk.Application.Features.Admin.Users.Dtos;

namespace HelpDesk.Application.Features.Admin.Users.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Department).MaximumLength(100);
        RuleFor(x => x.JobTitle).MaximumLength(100);
    }
}
