using Microsoft.AspNetCore.Mvc;
using NetatmoProxy.Configuration;
using NetatmoProxy.Model;
using NetatmoProxy.Model.Netatmo;
using NetatmoProxy.Services;
using Prometheus;

namespace NetatmoProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisplayController : ControllerBase
    {
        private static readonly Gauge BatteryLevel = Metrics.CreateGauge("sensor_battery_level", "Battery level for a given sensor", labelNames: new[] { "module_name" });
        private static readonly Gauge TemperatureLevel = Metrics.CreateGauge("sensor_temperature_level", "Temperature value for sensor", labelNames: new[] { "module_name", "in_or_out" });
        private static readonly Gauge WindLevel = Metrics.CreateGauge("sensor_wind_level", "Wind strengt for sensor", labelNames: new[] { "module_name", "wind_or_gust" });
        private static readonly Gauge WindAngle = Metrics.CreateGauge("sensor_wind_angle", "Wind angle for sensor", labelNames: new[] { "module_name", "wind_or_gust" });
        private readonly ILogger<DisplayController> _logger;
        private readonly NetatmoApiConfig _config;
        private readonly INetatmoApiService _netatmoApiService;
        private readonly IDayNightService _dayNightService;

        public DisplayController(ILogger<DisplayController> logger, NetatmoApiConfig config, INetatmoApiService netatmoApiService, IDayNightService dayNightService)
        {
            _logger = logger;
            _config = config;
            _netatmoApiService = netatmoApiService;
            _dayNightService = dayNightService;
        }

        [HttpGet]
        public async Task<Display> Get()
        {
            try
            {
                var stationData = await _netatmoApiService.GetStationsDataAsync(new Model.Netatmo.GetStationsDataRequest());

                IEnumerable<Device> indoorModules = stationData?.Body?.Devices;
                var outdoorTemperatureModules = new List<Module>();
                var outdoorWindModules = new List<Module>();
                var batteryIndicators = new List<BatteryIndicator>();

                // TODO: Use Gauges with descriptive naming (see home-assitant entities for Netatmo)
                // Probably use lables to be able to differensiate between indoor and outdoor modules etc

                foreach (var indoorModule in indoorModules)
                {
                    outdoorTemperatureModules.AddRange(
                        from m in indoorModule.Modules
                        where _config.Modules.Contains(m.ModuleName, StringComparer.InvariantCultureIgnoreCase)
                        && m.DataType.Contains("Temperature") && m.DashboardData != null
                        select m
                        );
                    outdoorWindModules.AddRange(
                        from m in indoorModule.Modules
                        where _config.Modules.Contains(m.ModuleName, StringComparer.InvariantCultureIgnoreCase)
                        && m.DataType.Contains("Wind") && m.DashboardData != null
                        select m
                        );
                    batteryIndicators.AddRange(
                        from m in indoorModule.Modules
                        where _config.Modules.Contains(m.ModuleName, StringComparer.InvariantCultureIgnoreCase)
                        select new BatteryIndicator
                        {
                            ModuleName = m.ModuleName,
                            BatteryLevel = m.BatteryPercent
                        });
                }

                // Exclude indoor modules not configured for display
                indoorModules = (from d in indoorModules
                                 where _config.Modules.Contains(d.ModuleName)
                                 && d.DashboardData != null
                                 select d).ToList();

                var sunOrMoon = await _dayNightService.IsSunOrMoonAsync();

                string FormatTemperature(decimal temperature)
                {
                    return $"{temperature.ToString("n1")}°C";
                }

                string FormatDateTime(int dateValue)
                {
                    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(dateValue).ToString("dd.MM HH:mm");
                }

                string FormatWindStrength(decimal strenghInKmPerHour)
                {
                    return $"{(strenghInKmPerHour / 3.6m).ToString("n1")}m/s";
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
                        SunOrMoon = sunOrMoon,
                        BatteryIndicators = batteryIndicators.ToArray()
                    };
                }

                Widget CreateWindWidget(string name, DashboardData data, int batteryPercent)
                {
                    return new Widget
                    {
                        Type = "wind",
                        Description = name,
                        Value = "Vind".Equals(name) ? FormatWindStrength(data.WindStrength) : FormatWindStrength(data.GustStrength),
                        Angle = "Vind".Equals(name) ? data.WindAngle.ToString() : data.GustAngle.ToString(),
                        MaxValue = FormatWindStrength(data.MaxWindStrength),
                        MaxAngle = data.MaxWindAngle.ToString(),
                        MaxTime = FormatDateTime(data.DateMaxWind),
                        BatteryLevel = batteryPercent
                    };
                }

                var widgets = new List<Widget>();
                widgets.AddRange(indoorModules.Select(m => CreateTemperatureWidget(m.ModuleName, m.DashboardData)));
                widgets.AddRange(outdoorTemperatureModules.Select(m => CreateTemperatureWidget(m.ModuleName, m.DashboardData)));
                widgets.AddRange(outdoorTemperatureModules.Select(m => CreateHumidityWidget(m.ModuleName, m.DashboardData, m.BatteryPercent)));
                widgets.AddRange(outdoorWindModules.Select(m => CreateWindWidget("Vind", m.DashboardData, m.BatteryPercent)));
                widgets.AddRange(outdoorWindModules.Select(m => CreateWindWidget("Kast", m.DashboardData, m.BatteryPercent)));

                // Export metrics for Promotheus
                ExportMetrics(indoorModules, outdoorTemperatureModules, outdoorWindModules);

                return new Display
                {
                    Widgets = widgets
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred calling any of the APIs for the Display data");
            }

            return null;
        }

        private void ExportMetrics(IEnumerable<Device> indoorModules, IEnumerable<Module>outdoorTemperatureModules, IEnumerable<Module> outdoorWindModules)
        {
            foreach (var device in indoorModules)
            {
                TemperatureLevel.WithLabels(device.ModuleName, "in").Set(Convert.ToDouble(device.DashboardData.Temperature));
            }

            foreach (var module in outdoorTemperatureModules)
            {
                BatteryLevel.WithLabels(module.ModuleName).Set(module.BatteryPercent);
                string inOrOut = "NAModule4".Equals(module.Type, StringComparison.InvariantCultureIgnoreCase) ? "in" : "out";
                TemperatureLevel.WithLabels(module.ModuleName, inOrOut).Set(Convert.ToDouble(module.DashboardData.Temperature));
            }

            foreach (var module in outdoorWindModules)
            {
                WindLevel.WithLabels(module.ModuleName, "wind").Set(module.DashboardData.WindStrength);
                WindAngle.WithLabels(module.ModuleName, "wind").Set(module.DashboardData.WindAngle);
                WindLevel.WithLabels(module.ModuleName, "gust").Set(module.DashboardData.GustStrength);
                WindAngle.WithLabels(module.ModuleName, "gust").Set(module.DashboardData.WindAngle);
            }
        }
    }
}
