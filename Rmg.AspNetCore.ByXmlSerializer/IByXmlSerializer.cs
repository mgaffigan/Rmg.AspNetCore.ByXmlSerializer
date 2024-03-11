using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Rmg.AspNetCore;

[JsonConverter(typeof(SkipConverterFactory))]
public struct ByXmlSerializer<T> : IByXmlSerializer
{
    internal static XmlSerializer Serializer { get; } = new XmlSerializer(typeof(T));

    public ByXmlSerializer(T value)
    {
        this.Value = value;
    }

    public T Value { get; }

    public static implicit operator ByXmlSerializer<T>(T value) => new(value);
    public static implicit operator T(ByXmlSerializer<T> value) => value.Value;

    void IByXmlSerializer.Write(XmlWriter xw) => Serializer.Serialize(xw, Value);
    internal static ByXmlSerializer<T> Deserialize(Stream s)
    {
        return (T)Serializer.Deserialize(s)!;
    }
}

public interface IByXmlSerializer
{
    void Write(XmlWriter xw);
}
