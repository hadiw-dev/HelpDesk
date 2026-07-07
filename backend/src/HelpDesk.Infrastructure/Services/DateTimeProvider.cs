using HelpDesk.Application.Common.Interfaces;

namespace HelpDesk.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
