using FluentValidation;
using HelpDesk.Application.Features.Tickets.Dtos;

namespace HelpDesk.Application.Features.Tickets.Validators;

public class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PriorityId).NotEmpty();
        RuleFor(x => x.StatusId).NotEmpty();
    }
}
