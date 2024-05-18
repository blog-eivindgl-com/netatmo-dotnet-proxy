using Microsoft.AspNetCore.Mvc;
using NetatmoProxy.Configuration;
using NetatmoProxy.Model;

namespace NetatmoProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationController : Controller
    {
        private readonly ILogger<SimulationController> _logger;
        private readonly SimulationConfig _config;

        public SimulationController(ILogger<SimulationController> logger, SimulationConfig config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpGet]
        public async Task<Display> Get()
        {
            try
            {

                decimal GenerateRandomDecimal(decimal minValue, decimal maxValue)
                {
                    decimal range = maxValue - minValue;

                    // Generate a random double between 0.0 and 1.0
                    double randomDouble = Random.Shared.NextDouble();

                    // Scale the random double to fit within the desired range
                    decimal randomDecimal = (decimal)(randomDouble * (double)range) + minValue;

                    return randomDecimal;
                }

                string GenerateRandomTrend()
                {
                    switch(Random.Shared.Next(0, 2))
                    {
                        case 1:
                            return "up";
                        case 2:
                            return "down";
                        default:
                            return "stable";
                    }
                }

                string GenerateRandomSunOrMoon()
                {
                    switch(Random.Shared.Next(0, 2))
                    {
                        case 1:
                            return "sun";
                        default:
                            return "moon";
                    }
                }

                DateTime GenereateRandomTime()
                {
                    // random number of seconds last 24h
                    return DateTime.Now.AddSeconds(-1 * Random.Shared.Next(0, 86400));
                }

                string FormatTemperature(decimal temperature)
                {
                    return $"{temperature.ToString("n1")}°C";
                }

                string FormatDateTimeFromInt(int dateValue)
                {
                    return FormatDateTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(dateValue));
                }

                string FormatDateTime(DateTime dateValue)
                {
                    return dateValue.ToString("dd.MM HH:mm");
                }

                string FormatWindStrength(decimal strenghInKmPerHour)
                {
                    return $"{(strenghInKmPerHour / 3.6m).ToString("n1")}m/s";
                }

                var temperatureWidgets = new List<Widget>();
                var humidityWidgets = new List<Widget>();
                var windWidgets = new List<Widget>();

                if (_config.Widgets == null)
                {
                    _config.Widgets = new Widget[]
                    {
                        new Widget
                        {
                            Type = "temperature",
                            Description = "test1",
                            MinValue = "-10",
                            MaxValue = "30"
                        },
                        new Widget
                        {
                            Type = "humidity",
                            Description = "test2",
                            MinValue = "-10",
                            MaxValue = "30"
                        },
                        new Widget
                        {
                            Type = "wind",
                            Description = "test3"
                        }
                    };
                }

                foreach (var widget in _config.Widgets)
                {
                    if ("temperature".Equals(widget.Type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        decimal minValue = GenerateRandomDecimal(-10, 10);
                        decimal maxValue = GenerateRandomDecimal(10, 30);
                        if (!string.IsNullOrWhiteSpace(widget.MinValue))
                        {
                            decimal.TryParse(widget.MinValue, out minValue);
                        }
                        if (!string.IsNullOrWhiteSpace(widget.MaxValue))
                        {
                            decimal.TryParse(widget.MaxValue, out maxValue);
                        }
                        var value = GenerateRandomDecimal(minValue, maxValue);
                        temperatureWidgets.Add(new Widget
                        {
                            Type = widget.Type,
                            Description = widget.Description,
                            Value = FormatTemperature(value),
                            MinValue = FormatTemperature(GenerateRandomDecimal(minValue, value)),
                            MaxValue = FormatTemperature(GenerateRandomDecimal(value, maxValue)),
                            MinTime = FormatDateTime(GenereateRandomTime()),
                            MaxTime = FormatDateTime(GenereateRandomTime()),
                            Trend = GenerateRandomTrend(),
                            BatteryLevel = Random.Shared.Next(0, 100)
                        });
                    }
                    else if ("humidity".Equals(widget.Type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        decimal minValue = GenerateRandomDecimal(-10, 10);
                        decimal maxValue = GenerateRandomDecimal(10, 30);
                        if (!string.IsNullOrWhiteSpace(widget.MinValue))
                        {
                            decimal.TryParse(widget.MinValue, out minValue);
                        }
                        if (!string.IsNullOrWhiteSpace(widget.MaxValue))
                        {
                            decimal.TryParse(widget.MaxValue, out maxValue);
                        }
                        humidityWidgets.Add(new Widget
                        {
                            Type = widget.Type,
                            Description = widget.Description,
                            Value = Random.Shared.Next(0, 100).ToString(),
                            OutTemp = GenerateRandomDecimal(minValue, maxValue),
                            SunOrMoon = GenerateRandomSunOrMoon(),
                            BatteryLevel = Random.Shared.Next(0, 100)
                        });
                    }
                    else if ("wind".Equals(widget.Type, StringComparison.InvariantCultureIgnoreCase))
                    {

                        windWidgets.Add(new Widget
                        {
                            Type = widget.Type,
                            Description = widget.Description,
                            Value = FormatWindStrength(GenerateRandomDecimal(0, 42)),
                            Angle = Random.Shared.Next(0, 360).ToString(),
                            MaxValue = FormatWindStrength(GenerateRandomDecimal(10, 50)),
                            MaxAngle = Random.Shared.Next(0, 360).ToString(),
                            MaxTime = FormatDateTime(GenereateRandomTime()),
                            BatteryLevel = Random.Shared.Next(0, 100)
                        });
                    }
                }

                return new Display
                {
                    Widgets = temperatureWidgets.ToArray().Union(humidityWidgets.ToArray()).Union(windWidgets.ToArray())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred simulating APIs for the Display data");
            }

            return null;
        }
    }
}
