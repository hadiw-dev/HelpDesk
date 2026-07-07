using FluentValidation;
using HelpDesk.Application.Features.Admin.Users.Dtos;
using HelpDesk.Domain.Identity;

namespace HelpDesk.Application.Features.Admin.Users.Validators;

public class ChangeUserRoleRequestValidator : AbstractValidator<ChangeUserRoleRequest>
{
    public ChangeUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => RoleNames.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}.");
    }
}
