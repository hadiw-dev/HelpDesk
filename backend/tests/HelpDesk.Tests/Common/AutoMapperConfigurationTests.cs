using AutoMapper;
using HelpDesk.Application;

namespace HelpDesk.Tests.Common;

public class AutoMapperConfigurationTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(DependencyInjection).Assembly));

        configuration.AssertConfigurationIsValid();
    }
}
