using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Rmg.AspNetCore;

public static class ByXmlSerializerExtensions
{
    public static IMvcBuilder AddByXmlSerializerFormatters(this IMvcBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ByXmlSerializerMvcOptionsSetup>());
        return builder;
    }

    private sealed class ByXmlSerializerMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions opts)
        {
            opts.OutputFormatters.Insert(0, new ByXmlSerializerOutputFormatter());
            opts.InputFormatters.Insert(0, new ByXmlSerializerInputFormatter());
        }
    }

    public static ByXmlSerializer<T> ByXmlSerializer<T>(this ControllerBase _, T value)
    {
        return value;
    }
}
