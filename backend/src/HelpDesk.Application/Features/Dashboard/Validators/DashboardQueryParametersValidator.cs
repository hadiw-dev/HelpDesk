using FluentValidation;
using HelpDesk.Application.Features.Dashboard.Dtos;

namespace HelpDesk.Application.Features.Dashboard.Validators;

public class DashboardQueryParametersValidator : AbstractValidator<DashboardQueryParameters>
{
    public DashboardQueryParametersValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom <= x.DateTo)
            .WithMessage("DateFrom must be on or before DateTo.");
    }
}
