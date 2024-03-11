using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Xml;

namespace Rmg.AspNetCore;

internal class ByXmlSerializerOutputFormatter : TextOutputFormatter
{
    public ByXmlSerializerOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanWriteType(Type type)
        => typeof(IByXmlSerializer).IsAssignableFrom(type);

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var bxs = (IByXmlSerializer)context.Object!;
        await using var fbws = new FileBufferingWriteStream();
        await using (var tw = context.WriterFactory(fbws, selectedEncoding))
        {
            using var xw = XmlWriter.Create(tw, new XmlWriterSettings() { Encoding = selectedEncoding, Indent = true });
            bxs.Write(xw);
        }

        await fbws.DrainBufferAsync(context.HttpContext.Response.Body);
    }
}
