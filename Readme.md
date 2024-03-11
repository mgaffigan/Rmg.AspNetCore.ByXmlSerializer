# ByXmlSerializer

[![Nuget](https://img.shields.io/nuget/v/Rmg.AspNetCore.ByXmlSerializer)](https://www.nuget.org/packages/Rmg.AspNetCore.ByXmlSerializer)

Formatter which allows mixed use of `XmlSerializer`, `DataContractSerializer`, and `JsonSerializer`.  Each 
action can "opt-in" to `XmlSerializer` by returning `ByXmlSerializer<T>` (or `ActionResult<ByXmlSerializer<T>>` et alia).

## Example use

Add a reference in csproj:

```xml
<ItemGroup>
    <PackageReference Include="Rmg.AspNetCore.ByXmlSerializer" Version="1.0.0" />
</ItemGroup>
```

Enable the formatter by adding it before adding DCS in `Program.cs`:

```csharp
using Rmg.AspNetCore;

// ...snip...

builder.Services.AddControllers()
    .AddByXmlSerializerFormatters()
    .AddXmlDataContractSerializerFormatters();

// ...snip...
```

"Mark" specific actions as preferring `XmlSerializer` by returning or accepting `ByXmlSerializer<T>`:

```csharp
[HttpGet("xs1")]
public ByXmlSerializer<WeatherForecast> GetExample1()
{
    // ByXmlSerializer<T> is implicitly creatable from T
    return new WeatherForecast();
}

[HttpGet("xs2")]
public ActionResult<ByXmlSerializer<WeatherForecast>> GetExample2()
{
    // or use an extension method on ControllerBase to coerce type
    return this.ByXmlSerailizer(new WeatherForecast());
}

[HttpGet("xs3")]
public ActionResult<ByXmlSerializer<WeatherForecast>> GetExample3()
{
    // or explicitly construct
    return StatusCode(400, new ByXmlSerializer<WeatherForecast>(new ()));
}

[HttpPost("pe1")]
public ByXmlSerializer<WeatherForecast> PostExample1([FromBody] ByXmlSerializer<ForecastArea> area)
{
    // ByXmlSerializer<T> is implicitly unwrappable to T
    ForecastArea unwrappedArea = area;
    return new WeatherForecast(unwrappedArea);
}

[HttpPost("pe2")]
public ByXmlSerializer<WeatherForecast> GetExample5([FromBody] ByXmlSerializer<ForecastArea> area)
{
    // or use .Value directly
    return new WeatherForecast(area.Value);
}
```

Methods marked with `ByXmlSerializer<T>` can still use other formatters (e.g.: System.Text.Json) without 
issue.  Only formatters which also read `text/xml` or `application/xml` are superseded.