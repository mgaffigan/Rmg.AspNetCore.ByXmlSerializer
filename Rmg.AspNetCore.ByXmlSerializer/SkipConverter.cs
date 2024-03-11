using System.Text.Json.Serialization;
using System.Text.Json;

namespace Rmg.AspNetCore;

internal sealed class SkipConverter<T> : JsonConverter<ByXmlSerializer<T>>
{
    private readonly JsonConverter<T> tConverter;

    public SkipConverter(JsonSerializerOptions options)
    {
        tConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
    }

    public override ByXmlSerializer<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return tConverter.Read(ref reader, typeof(T), options)!;
    }

    public override void Write(Utf8JsonWriter writer, ByXmlSerializer<T> value, JsonSerializerOptions options)
    {
        tConverter.Write(writer, value, options);
    }
}

internal sealed class SkipConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;
        if (typeToConvert.GetGenericTypeDefinition() != typeof(ByXmlSerializer<>)) return false;
        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var tValue = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)Activator.CreateInstance(typeof(SkipConverter<>).MakeGenericType(tValue), [options])!;
    }
}
