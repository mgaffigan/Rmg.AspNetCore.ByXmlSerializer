using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
#if NET8_0
using Microsoft.AspNetCore.Internal;
#endif
using Microsoft.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace Rmg.AspNetCore;

internal class ByXmlSerializerInputFormatter : TextInputFormatter
{
    public ByXmlSerializerInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanReadType(Type type)
        => typeof(IByXmlSerializer).IsAssignableFrom(type);

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        // https://source.dot.net/#Microsoft.AspNetCore.Mvc.Formatters.Xml/XmlSerializerInputFormatter.cs,81
        var request = context.HttpContext.Request;
        await using var readStream = await GetSeekableRequestStream(request);

        try
        {
            var result = context.ModelType.InvokeMember("Deserialize", 
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod,
                null, null, [readStream])!;
            return InputFormatterResult.Success(result);
        }
        // XmlSerializer wraps actual exceptions (like FormatException or XmlException) into an InvalidOperationException
        // https://github.com/dotnet/corefx/blob/master/src/System.Private.Xml/src/System/Xml/Serialization/XmlSerializer.cs#L652
        catch (InvalidOperationException ex)
        {
            throw new InputFormatterException("Error deserializing input data", ex.InnerException!);
        }
    }

    private static async Task<Stream> GetSeekableRequestStream(HttpRequest request)
    {
        var body = request.Body;

        // XmlSerializer does synchronous reads, which will throw
        // Deal with lazy input buffering
        if (body.CanSeek)
        {
            var position = body.Position;
            await body.DrainAsync(CancellationToken.None);
            body.Position = position;
            return new NonDisposableStream(body);
        }
        // Buffer if non-buffered
        else
        {
            var readStream = new FileBufferingReadStream(body, 1024 * 30);
            try
            {
                await readStream.DrainAsync(CancellationToken.None);
                readStream.Position = 0L;
                return readStream;
            }
            catch
            {
                readStream.Dispose();
                throw;
            }
        }
    }

#if !NET8_0
    private class NonDisposableStream : Stream
    {
        private readonly Stream BaseStream;

        public NonDisposableStream(Stream baseStream)
        {
            this.BaseStream = baseStream;
        }

        protected override void Dispose(bool disposing)
        {
            // nop
        }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }
    }
#endif
}
