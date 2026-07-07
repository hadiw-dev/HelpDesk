using FluentValidation;
using HelpDesk.Application.Features.Admin.ActivityLogs.Dtos;

namespace HelpDesk.Application.Features.Admin.ActivityLogs.Validators;

public class ActivityLogQueryParametersValidator : AbstractValidator<ActivityLogQueryParameters>
{
    public ActivityLogQueryParametersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x)
            .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom <= x.DateTo)
            .WithMessage("DateFrom must be on or before DateTo.");
    }
}
