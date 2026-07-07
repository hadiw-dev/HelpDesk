using FluentValidation;
using HelpDesk.Application.Features.Admin.Lookups.Dtos;

namespace HelpDesk.Application.Features.Admin.Lookups.Validators;

public class LookupUpsertRequestValidator : AbstractValidator<LookupUpsertRequest>
{
    public LookupUpsertRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
