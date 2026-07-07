using FluentValidation;
using HelpDesk.Application.Features.Admin.Settings.Dtos;

namespace HelpDesk.Application.Features.Admin.Settings.Validators;

public class UpdateSystemSettingsRequestValidator : AbstractValidator<UpdateSystemSettingsRequest>
{
    public UpdateSystemSettingsRequestValidator()
    {
        RuleFor(x => x.SiteName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MaxFileUploadSizeMb).InclusiveBetween(1, 100);
        RuleFor(x => x.AllowedFileExtensions).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DefaultPageSize).InclusiveBetween(1, 100);
    }
}
