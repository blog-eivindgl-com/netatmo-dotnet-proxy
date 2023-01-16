﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetatmoProxy.Configuration;
using NetatmoProxy.Model;
using NetatmoProxy.Model.MetSunrise;
using System.Globalization;
using System.Net.Http.Json;

namespace NetatmoProxy.Services
{
    public class MetSunriseApiService : IDayNightService
    {
        public const string UserAgentString = @"NetatmoProxy/1.0 github.com/blog-eivindgl-com/netatmo-dotnet-proxy";
        public const string BaseUri = @"https://api.met.no/weatherapi/sunrise/2.0/";
        private readonly string _latitude;
        private readonly string _longitude;
        private readonly string _height;
        private readonly int _days;
        private readonly ILogger<MetSunriseApiService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memCache;
        private readonly INowService _nowService;

        public MetSunriseApiService(MetSunriseApiConfig config, ILogger<MetSunriseApiService> logger, HttpClient httpClient, IMemoryCache memCache, INowService nowService)
        {
            _latitude = FormatDecimal(config.Latitude);
            _longitude = FormatDecimal(config.Longitude);
            _height = FormatDecimal(config.Height);
            _days = 15;
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

            if (!_memCache.TryGetValue(now.Date, out sunriseSunset))
            {
                // Request as many days as possible
                var offset = TimeZoneInfo.Local.GetUtcOffset(now).ToString(@"hh\:mm");
                offset = TimeZoneInfo.Local.BaseUtcOffset < TimeSpan.Zero ? $"-{offset}" : $"+{offset}";
                var request = $".json?lat={_latitude}&lon={_longitude}&height={_height}&date={now.ToString("yyyy-MM-dd")}&offset={offset}&days={_days}";
                
                var response = await _httpClient.GetAsync(request);

                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch
                {
                    _logger.LogError(await response.Content.ReadAsStringAsync());
                    throw;
                }

                var sunriseResponse = await response.Content.ReadFromJsonAsync<SunriseResponse>();

                // Cache values for all days retrieved and return today's values
                if (sunriseResponse?.Location?.Time != null)
                {
                    foreach (var day in sunriseResponse.Location.Time)
                    {
                        if (day.Sunrise != null && day.Sunset != null)
                        {
                            DateTime key = DateTime.ParseExact(day.Date, "yyyy-MM-dd", CultureInfo.CurrentUICulture, DateTimeStyles.AssumeLocal);
                            var value = new SunriseSunset { Sunrise = day.Sunrise.Time, Sunset = day.Sunset.Time };
                            var cacheEntry = _memCache.CreateEntry(key);
                            cacheEntry.Value = value;
                            cacheEntry.AbsoluteExpiration = key.AddDays(1);  // cache one day longer than the actual date
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
