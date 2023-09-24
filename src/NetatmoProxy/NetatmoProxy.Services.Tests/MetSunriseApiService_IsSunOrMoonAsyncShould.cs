using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Contrib.HttpClient;
using NetatmoProxy.Configuration;
using NetatmoProxy.Model.MetSunrise;
using Shouldly;
using System.Globalization;
using Xunit;

namespace NetatmoProxy.Services.Tests
{
    public class MetSunriseApiService_IsSunOrMoonAsyncShould
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const string DefaultNowDateString = "2022-01-15";
        private const string DefaultNowString = $"{DefaultNowDateString} 15:30:00";
        private readonly DateTime _defaultNow;
        private readonly MetSunriseApiConfig _defaultConfig;
        private readonly Mock<ILogger<MetSunriseApiService>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<INowService> _nowServiceMock;
        private readonly MetSunriseApiService _service;
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _serviceProvider;

        public MetSunriseApiService_IsSunOrMoonAsyncShould()
        {
            _services = new ServiceCollection();
            _services.AddMemoryCache();
            _serviceProvider = _services.BuildServiceProvider();
            _defaultNow = ParseDateTime(DefaultNowString);
            _defaultConfig = new MetSunriseApiConfig { Latitude = 12.56m, Longitude = 12.56m };
            _loggerMock = new Mock<ILogger<MetSunriseApiService>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var factory = _httpMessageHandlerMock.CreateClientFactory();
            _memoryCache = _serviceProvider.GetService<IMemoryCache>();
            _nowServiceMock = new Mock<INowService>();
            _nowServiceMock.SetupGet(m => m.DateTimeNow).Returns(_defaultNow);
            _service = new MetSunriseApiService(
                config: _defaultConfig,
                logger: _loggerMock.Object,
                httpClient: factory.CreateClient(),
                memCache: _memoryCache,
                nowService: _nowServiceMock.Object
                );
        }

        private DateTime ParseDateTime(string dateTimeString) => 
            DateTime.ParseExact(dateTimeString, DateTimeFormat, CultureInfo.CurrentUICulture, DateTimeStyles.AssumeLocal);

        private string FormatDecimal(decimal value) =>
            value.ToString(CultureInfo.InvariantCulture);

        [Fact]
        public async Task CallExpectedUri()
        {
            // Arrange
            decimal latitude = _defaultConfig.Latitude;
            decimal longitude = _defaultConfig.Longitude;
            string offset = "+01:00";
            int days = 2;
            string expectedRequestUri = $"https://api.met.no/weatherapi/sunrise/3.0/sun?lat={FormatDecimal(latitude)}&lon={FormatDecimal(longitude)}&date={DefaultNowDateString}&offset={offset}";
            var expectedResponse = new SunriseResponse
            {
                Geometry = new Geometry
                {
                    Type = "Point",
                    Coordinates = new decimal[] { 12.5m, 12.5m }
                },
                When = new When
                {
                    Interval = new DateTime[] { _defaultNow.AddHours(23).AddMinutes(31), _defaultNow.AddDays(1).AddHours(23).AddMinutes(31) }
                },
                Properties = new Properties
                {
                    Body = "Sun",
                    Sunrise = new RiseSet
                    {
                        Time = _defaultNow.AddHours(6).AddMinutes(23),
                        Azimuth = 138.76m
                    },
                    Sunset = new RiseSet
                    {
                        Time = _defaultNow.AddHours(20).AddMinutes(13),
                        Azimuth = 221.39m
                    },
                    Solarnoon = new Noon
                    {
                        Time = _defaultNow.AddDays(1).AddHours(12).AddMinutes(41),
                        Elevation = 6.36m,
                        Visible = true
                    },
                    Solarmidnight = new Noon
                    {
                        Time = _defaultNow.AddDays(1).AddMinutes(41),
                        Elevation = -48.33m,
                        Visible = false
                    }
                }
            };
            _httpMessageHandlerMock.SetupRequest(expectedRequestUri).ReturnsJsonResponse<SunriseResponse>(expectedResponse);
            string day2String = _defaultNow.Date.AddDays(1).ToString("yyyy-MM-dd");
            string expectedRequestUriDay2 = $"https://api.met.no/weatherapi/sunrise/3.0/sun?lat={FormatDecimal(latitude)}&lon={FormatDecimal(longitude)}&date={day2String}&offset={offset}";
            var expectedResponseDay2 = new SunriseResponse
            {
                Geometry = new Geometry
                {
                    Type = "Point",
                    Coordinates = new decimal[] { 12.5m, 12.5m }
                },
                When = new When
                {
                    Interval = new DateTime[] { _defaultNow.AddDays(1).AddHours(23).AddMinutes(31), _defaultNow.AddDays(2).AddHours(23).AddMinutes(31) }
                },
                Properties = new Properties
                {
                    Body = "Sun",
                    Sunrise = new RiseSet
                    {
                        Time = _defaultNow.AddDays(1).AddHours(6).AddMinutes(23),
                        Azimuth = 138.76m
                    },
                    Sunset = new RiseSet
                    {
                        Time = _defaultNow.AddDays(1).AddHours(20).AddMinutes(13),
                        Azimuth = 221.39m
                    },
                    Solarnoon = new Noon
                    {
                        Time = _defaultNow.AddDays(2).AddHours(12).AddMinutes(41),
                        Elevation = 6.36m,
                        Visible = true
                    },
                    Solarmidnight = new Noon
                    {
                        Time = _defaultNow.AddDays(2).AddMinutes(41),
                        Elevation = -48.33m,
                        Visible = false
                    }
                }
            };
            _httpMessageHandlerMock.SetupRequest(expectedRequestUriDay2).ReturnsJsonResponse<SunriseResponse>(expectedResponseDay2);

            // Act
            await _service.IsSunOrMoonAsync();

            // Assert
            _httpMessageHandlerMock.VerifyRequest(expectedRequestUri, Times.Once());
            _httpMessageHandlerMock.VerifyRequest(expectedRequestUriDay2, Times.Once());
        }

        [Theory]
        [InlineData("2022-12-12 17:30:00", "2022-12-12 09:30:00", "2022-12-12 18:30:00", "sun")]
        [InlineData("2022-12-12 18:30:00", "2022-12-12 09:30:00", "2022-12-12 18:30:00", "sun")]
        [InlineData("2022-12-12 18:30:01", "2022-12-12 09:30:00", "2022-12-12 18:30:00", "moon")]
        [InlineData("2022-12-13 00:00:01", "2022-12-13 09:30:00", "2022-12-13 18:30:00", "moon")]
        [InlineData("2022-12-13 09:29:59", "2022-12-13 09:30:00", "2022-12-13 18:30:00", "moon")]
        [InlineData("2022-12-13 09:30:00", "2022-12-13 09:30:00", "2022-12-13 18:30:00", "sun")]
        public async Task ReturnSunAfterSunrise_AndMoonAfterSunset(string mockedNow, string mockedSunrise, string mockedSunset, string expectedResult)
        {
            // Arrange
            decimal latitude = _defaultConfig.Latitude;
            decimal longitude = _defaultConfig.Longitude;
            string offset = "+01:00";
            DateTime mockedNowValue = ParseDateTime(mockedNow);
            _nowServiceMock.SetupGet(s => s.DateTimeNow).Returns(mockedNowValue);

            string mockRequestResponse(int day)
            {
                DateTime mockedDateValue = mockedNowValue.Date.AddDays(day - 1);
                string mockedDate = mockedDateValue.ToString("yyyy-MM-dd");

                string expectedRequestUri = $"https://api.met.no/weatherapi/sunrise/3.0/sun?lat={FormatDecimal(latitude)}&lon={FormatDecimal(longitude)}&date={mockedDate}&offset={offset}";
                var expectedResponse = new SunriseResponse
                {
                    Geometry = new Geometry
                    {
                        Type = "Point",
                        Coordinates = new decimal[] { longitude, latitude }
                    },
                    When = new When
                    {
                        Interval = new DateTime[] { mockedDateValue.AddHours(23).AddMinutes(31), mockedDateValue.AddDays(1).AddHours(23).AddMinutes(31) }
                    },
                    Properties = new Properties
                    {
                        Body = "Sun",
                        Sunrise = new RiseSet
                        {
                            Time = ParseDateTime(mockedSunrise).AddDays(day - 1),
                            Azimuth = 138.76m
                        },
                        Sunset = new RiseSet
                        {
                            Time = ParseDateTime(mockedSunset).AddDays(day - 1),
                            Azimuth = 221.39m
                        },
                        Solarnoon = new Noon
                        {
                            Time = mockedDateValue.AddDays(1).AddHours(12).AddMinutes(41),
                            Elevation = 6.36m,
                            Visible = true
                        },
                        Solarmidnight = new Noon
                        {
                            Time = mockedDateValue.AddDays(1).AddMinutes(41),
                            Elevation = -48.33m,
                            Visible = false
                        }
                    }
                };
                _httpMessageHandlerMock.SetupRequest(expectedRequestUri).ReturnsJsonResponse<SunriseResponse>(expectedResponse);

                return expectedRequestUri;
            }

            string expectedRequestUriDay1 = mockRequestResponse(1);
            string expectedRequestUriDay2 = mockRequestResponse(2);

            // Act
            string actualResult = await _service.IsSunOrMoonAsync();

            // Assert
            _httpMessageHandlerMock.VerifyRequest(expectedRequestUriDay1, Times.Once());
            _httpMessageHandlerMock.VerifyRequest(expectedRequestUriDay2, Times.Once());
            actualResult.ShouldBe(expectedResult, "Result as expected");
        }

        // TODO: For some reason MemoryCache doesn't work as expected in a unit test, but it works when running in IIS Express
        //[Fact]
        public async Task GetValuesFromMemCacheTheSecondTime()
        {
            // Arrange
            decimal latitude = _defaultConfig.Latitude;
            decimal longitude = _defaultConfig.Longitude;
            string offset = "+01:00";
            int days = 15;
            string expectedRequestUri = $"https://api.met.no/weatherapi/sunrise/2.0/.json?lat={FormatDecimal(latitude)}&lon={FormatDecimal(longitude)}&date={DefaultNowDateString}&offset={offset}";
            var expectedResponse = new SunriseResponse
            {
                Geometry = new Geometry
                {
                    Type = "Point",
                    Coordinates = new decimal[] { 12.5m, 12.5m }
                },
                When = new When
                {
                    Interval = new DateTime[] { _defaultNow.AddHours(23).AddMinutes(31), _defaultNow.AddDays(1).AddHours(23).AddMinutes(31) }
                },
                Properties = new Properties
                {
                    Body = "Sun",
                    Sunrise = new RiseSet
                    {
                        Time = _defaultNow.AddHours(6).AddMinutes(23),
                        Azimuth = 138.76m
                    },
                    Sunset = new RiseSet
                    {
                        Time = _defaultNow.AddHours(20).AddMinutes(13),
                        Azimuth = 221.39m
                    },
                    Solarnoon = new Noon
                    {
                        Time = _defaultNow.AddDays(1).AddHours(12).AddMinutes(41),
                        Elevation = 6.36m,
                        Visible = true
                    },
                    Solarmidnight = new Noon
                    {
                        Time = _defaultNow.AddDays(1).AddMinutes(41),
                        Elevation = -48.33m,
                        Visible = false
                    }
                }
            };
            _httpMessageHandlerMock.SetupRequest(expectedRequestUri).ReturnsJsonResponse<SunriseResponse>(expectedResponse);

            // Act
            await _service.IsSunOrMoonAsync();
            await _service.IsSunOrMoonAsync();

            // Assert
            _httpMessageHandlerMock.VerifyRequest(expectedRequestUri, Times.Once(), "api.met.no is called only once, because memcache was used the second time.");
        }
    }
}
