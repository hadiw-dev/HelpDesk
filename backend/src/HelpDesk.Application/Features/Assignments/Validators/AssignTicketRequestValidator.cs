using FluentValidation;
using HelpDesk.Application.Features.Assignments.Dtos;

namespace HelpDesk.Application.Features.Assignments.Validators;

public class AssignTicketRequestValidator : AbstractValidator<AssignTicketRequest>
{
    public AssignTicketRequestValidator()
    {
        RuleFor(x => x.AssignedToUserId)
            .NotEqual(Guid.Empty)
            .When(x => x.AssignedToUserId.HasValue)
            .WithMessage("AssignedToUserId cannot be an empty GUID.");
    }
}
