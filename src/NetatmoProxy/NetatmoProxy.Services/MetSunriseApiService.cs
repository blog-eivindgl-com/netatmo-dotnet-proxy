using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetatmoProxy.Configuration;
using NetatmoProxy.Model;
using NetatmoProxy.Model.MetSunrise;
using Prometheus;
using System.Globalization;
using System.Net.Http.Json;

namespace NetatmoProxy.Services
{
    public class MetSunriseApiService : IDayNightService
    {
        public const string HttpClientName = "MetSunriseApiServiceHttpClient";
        public const string UserAgentString = @"NetatmoProxy/1.0 github.com/blog-eivindgl-com/netatmo-dotnet-proxy";
        public const string BaseUri = @"https://api.met.no/weatherapi/sunrise/3.0/";
        private static readonly Counter IsSunOrMoonCalls = Metrics.CreateCounter("metsunriseapiservice_issunormooncalls_total", "Number of times IsSunOrMoonAsync has been called.");
        private static readonly Counter MetApiCalls = Metrics.CreateCounter("metsunriseapiservice_externalapicalls_total", "Number of times the external Met API has been called.");
        private static readonly Counter MetApiErrors = Metrics.CreateCounter("metsunriseapiservice_externalapierrors_total", "Number of failed calls to the external Met API.");
        private readonly string _latitude;
        private readonly string _longitude;
        private readonly int _days;
        private readonly ILogger<MetSunriseApiService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memCache;
        private readonly INowService _nowService;

        public MetSunriseApiService(MetSunriseApiConfig config, ILogger<MetSunriseApiService> logger, HttpClient httpClient, IMemoryCache memCache, INowService nowService)
        {
            _latitude = FormatDecimal(config.Latitude);
            _longitude = FormatDecimal(config.Longitude);
            _days = 2;
            _logger = logger;
            _httpClient = httpClient;
            _memCache = memCache;
            _httpClient.BaseAddress = new Uri(BaseUri);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgentString);
            _nowService = nowService;
        }

        private string FormatDecimal(decimal value) =>
            value.ToString(CultureInfo.InvariantCulture);

        public async Task<string> IsSunOrMoonAsync()
        {
            var now = _nowService.DateTimeNow;
            SunriseSunset? sunriseSunset;
            IsSunOrMoonCalls.Inc();

            if (!_memCache.TryGetValue(now.Date, out sunriseSunset))
            {
                // Request the configured number of days
                var days = _days > 0 ? _days : 1;
                var dayCounter = 0;

                while (dayCounter++ < days)
                {
                    var offset = TimeZoneInfo.Local.GetUtcOffset(now).ToString(@"hh\:mm");
                    offset = TimeZoneInfo.Local.BaseUtcOffset < TimeSpan.Zero ? $"-{offset}" : $"+{offset}";
                    var requestDate = now.AddDays(dayCounter - 1);
                    var request = $"sun?lat={_latitude}&lon={_longitude}&date={requestDate.ToString("yyyy-MM-dd")}&offset={offset}";

                    MetApiCalls.Inc();
                    var response = await _httpClient.GetAsync(request);

                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch
                    {
                        MetApiErrors.CountExceptions(() => request);
                        _logger.LogError(await response.Content.ReadAsStringAsync());
                        throw;
                    }

                    var sunriseResponse = await response.Content.ReadFromJsonAsync<SunriseResponse>();

                    // Cache values for all days retrieved and return today's values
                    if ("sun".Equals(sunriseResponse?.Properties?.Body, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (sunriseResponse.Properties.Sunrise != null && sunriseResponse.Properties.Sunset != null)
                        {
                            var key = requestDate.Date;
                            var value = new SunriseSunset { Sunrise = sunriseResponse.Properties.Sunrise.Time, Sunset = sunriseResponse.Properties.Sunset.Time };
                            var cacheExpiracy = new MemoryCacheEntryOptions
                            {
                                AbsoluteExpiration = key.AddDays(1)
                            };
                            _memCache.Set(key, value, cacheExpiracy);
                            _logger.LogInformation($"Sunrise and sunset for {key.ToString("yyyy-MM-dd")}: {value.Sunrise} and {value.Sunset}");

                            if (key == now.Date)
                            {
                                // This is the one we're looking for today
                                sunriseSunset = value;
                            }
                        }
                    }
                }
            }

            return (sunriseSunset == null || (now >= sunriseSunset.Sunrise && now <= sunriseSunset.Sunset)) ? "sun" : "moon";
        }
    }
}
