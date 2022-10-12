using Microsoft.AspNetCore.Mvc;
using WSWebApi;

namespace WSWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        protected readonly IConfiguration config;
        protected readonly ILogger logger;
        protected readonly Service1 service1;

        private static readonly string[] Summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        public WeatherForecastController(
            IConfiguration config,
            ILoggerFactory loggerFactory,
            Service1 service1
        ) {
            this.config = config;
            this.logger = loggerFactory.CreateLogger<WeatherForecastController>();
            this.service1 = service1;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            await service1.DoSomething();

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}