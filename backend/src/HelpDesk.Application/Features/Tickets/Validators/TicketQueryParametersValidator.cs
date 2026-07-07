using FluentValidation;
using HelpDesk.Application.Features.Tickets.Dtos;

namespace HelpDesk.Application.Features.Tickets.Validators;

public class TicketQueryParametersValidator : AbstractValidator<TicketQueryParameters>
{
    private static readonly string[] AllowedSortFields =
        ["createdAt", "title", "priority", "status", "duedate", "ticketnumber"];

    public TicketQueryParametersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy)
            .Must(value => string.IsNullOrWhiteSpace(value) || AllowedSortFields.Contains(value.ToLowerInvariant()))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");
    }
}
