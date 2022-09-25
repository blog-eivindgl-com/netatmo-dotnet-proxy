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

        public DisplayController(INetatmoApiService netatmoApiService)
        {
            _netatmoApiService = netatmoApiService;
        }

        [HttpGet]
        public async Task<Display> Get()
        {
            var stationData = await _netatmoApiService.GetStationsDataAsync(new Model.Netatmo.GetStationsDataRequest());

            var indoor = stationData.Body.Devices.First();
            var outdoor = indoor.Modules.First();

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
                    BatteryLevel = batteryPercent
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
