using Microsoft.AspNetCore.Mvc;
using Rmg.AspNetCore;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DemoApp;

[ApiController]
[Route("forecast")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries
        = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    [HttpGet("normal")]
    public WeatherForecast[] GetWeatherForecast([FromQuery] ForecastArea? area = null)
    {
        return Enumerable.Range(1, 10).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("xs")]
    public ByXmlSerializer<WeatherForecast[]> GetWeatherForecastXmlSerializer()
    {
        return GetWeatherForecast();
    }

    [HttpPost("post")]
    public WeatherForecast[] PostWeatherForecast([FromBody] ForecastArea area)
    {
        return GetWeatherForecast().Take(area.Count).ToArray();
    }

    [HttpPost("postxs")]
    public async Task<ActionResult<ByXmlSerializer<WeatherForecast[]>>> PostWeatherForecastXs([FromBody] ByXmlSerializer<ForecastArea> area)
    {
        await Task.Delay(0);
        return this.ByXmlSerializer(PostWeatherForecast(area.Value));
    }
}

public class WeatherForecast
{
    [XmlAttribute]
    public DateTime Date { get; set; }

    [XmlAttribute]
    public int TemperatureC { get; set; }

    [XmlAttribute]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    [XmlAttribute]
    public string? Summary { get; set; }
}

[XmlRoot("ForecastArea")]
[DataContract(Namespace = "")]
public class ForecastArea
{
    [XmlAttribute]
    [DataMember]
    public int Count { get; set; }
}