using Microsoft.AspNetCore.Mvc;
using NetatmoProxy.Model;
using NetatmoProxy.Model.Netatmo;
using NetatmoProxy.Services;

namespace NetatmoProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisplayController : ControllerBase
    {
        private readonly INetatmoApiService _netatmoApiService;
        private readonly IDayNightService _dayNightService;

        public DisplayController(INetatmoApiService netatmoApiService, IDayNightService dayNightService)
        {
            _netatmoApiService = netatmoApiService;
            _dayNightService = dayNightService;
        }

        [HttpGet]
        public async Task<Display> Get()
        {
            var stationData = await _netatmoApiService.GetStationsDataAsync(new Model.Netatmo.GetStationsDataRequest());

            var indoor = stationData?.Body?.Devices?.FirstOrDefault();
            var outdoor = indoor?.Modules?.FirstOrDefault();

            var sunOrMoon = await _dayNightService.IsSunOrMoonAsync();

            string FormatTemperature(decimal temperature)
            {
                return $"{temperature.ToString("n1")}°C";
            }

            string FormatDateTime(int dateValue)
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(dateValue).ToString("dd.MM HH:mm");
            }

            Widget CreateTemperatureWidget(string name, DashboardData data)
            {
                return new Widget
                {
                    Type = "temperature",
                    Description = name,
                    Value = FormatTemperature(data.Temperature),
                    Trend = data.TempTrend,
                    MinValue = FormatTemperature(data.MinTemp),
                    MinTime = FormatDateTime(data.DateMinTemp),
                    MaxValue = FormatTemperature(data.MaxTemp),
                    MaxTime = FormatDateTime(data.DateMaxTemp)
                };
            }

            Widget CreateHumidityWidget(string name, DashboardData data, int batteryPercent)
            {
                return new Widget
                {
                    Type = "humidity",
                    Description = name,
                    Value = data.Humidity.ToString(),
                    OutTemp = data.Temperature,
                    BatteryLevel = batteryPercent,
                    SunOrMoon = sunOrMoon
                };
            }

            return new Display
            {
                Widgets = new Widget[]
                {
                    CreateTemperatureWidget(indoor.ModuleName, indoor.DashboardData),
                    CreateTemperatureWidget(outdoor.ModuleName, outdoor.DashboardData),
                    CreateHumidityWidget(outdoor.ModuleName, outdoor.DashboardData, outdoor.BatteryPercent)
                }
            };
        }
    }
}
