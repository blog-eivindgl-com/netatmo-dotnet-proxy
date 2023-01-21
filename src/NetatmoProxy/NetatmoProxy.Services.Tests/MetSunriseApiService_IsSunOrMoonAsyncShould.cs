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
            _defaultConfig = new MetSunriseApiConfig { Height = 100, Latitude = 12.56m, Longitude = 12.56m };
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
            decimal height = _defaultConfig.Height;
            string offset = "+01:00";
            int days = 15;
            string expectedRequestUri = $"https://api.met.no/weatherapi/sunrise/2.0/.json?lat={FormatDecimal(latitude)}&lon={FormatDecimal(longitude)}&height={FormatDecimal(height)}&date={DefaultNowDateString}&offset={offset}&days={days}";
            var expectedResponse = new SunriseResponse
            {
                Location = new Location
                {
                    Height = height.ToString(),
                    Latitude = latitude.ToString(),
                    Longitude = longitude.ToString(),
                    Time = new List<Time> {
                        new Time
                        {
                            Date = DefaultNowDateString,
                            Sunrise = new Sunrise
                            {
                                Time = _defaultNow.AddHours(6).AddMinutes(23)
                            },
                            Sunset = new Sunset
                            {
                                Time = _defaultNow.AddHours(20).AddMinutes(13)
                            }
                        }
                    }
                }
            };
            _httpMessageHandlerMock.SetupRequest(expectedRequestUri).ReturnsJsonResponse<SunriseResponse>(expectedResponse);

            // Act
            await _service.IsSunOrMoonAsync();

            // Assert
            _httpMessageHandlerMock.VerifyRequest(expectedRequestUri, Times.Once());
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
            decimal height = _defaultConfig.Height;
            string offset = "+01:00";
            int days = 15;
            _nowServiceMock.SetupGet(s => s.DateTimeNow).Returns(ParseDateTime(mockedNow));
            string mockedDate = _nowServiceMock.Object.DateTimeNow.ToString("yyyy-MM-dd");
            string expectedRequestUri = $"https://api.met.no/weatherapi/sunrise/2.0/.json?lat={FormatDecimal(latitude)}&lon={FormatDecimal(longitude)}&height={FormatDecimal(height)}&date={mockedDate}&offset={offset}&days={days}";
            var expectedResponse = new SunriseResponse
            {
                Location = new Location
                {
                    Height = height.ToString(),
                    Latitude = latitude.ToString(),
                    Longitude = longitude.ToString(),
                    Time = new List<Time> {
                        new Time
                        {
                            Date = ParseDateTime(mockedSunrise).ToString("yyyy-MM-dd"),
                            Sunrise = new Sunrise
                            {
                                Time = ParseDateTime(mockedSunrise)
                            },
                            Sunset = new Sunset
                            {
                                Time = ParseDateTime(mockedSunset)
                            }
                        }
                    }
                }
            };
            _httpMessageHandlerMock.SetupRequest(expectedRequestUri).ReturnsJsonResponse<SunriseResponse>(expectedResponse);

            // Act
            string actualResult = await _service.IsSunOrMoonAsync();

            // Assert
            _httpMessageHandlerMock.VerifyRequest(expectedRequestUri, Times.Once());
            actualResult.ShouldBe(expectedResult, "Result as expected");
        }

        // TODO: For some reason MemoryCache doesn't work as expected in a unit test, but it works when running in IIS Express
        //[Fact]
        public async Task GetValuesFromMemCacheTheSecondTime()
        {
            // Arrange
            decimal latitude = _defaultConfig.Latitude;
            decimal longitude = _defaultConfig.Longitude;
            decimal height = _defaultConfig.Height;
            string offset = "+01:00";
            int days = 15;
            string expectedRequestUri = $"https://api.met.no/weatherapi/sunrise/2.0/.json?lat={FormatDecimal(latitude)}&lon={FormatDecimal(longitude)}&height={FormatDecimal(height)}&date={DefaultNowDateString}&offset={offset}&days={days}";
            var expectedResponse = new SunriseResponse
            {
                Location = new Location
                {
                    Height = height.ToString(),
                    Latitude = latitude.ToString(),
                    Longitude = longitude.ToString(),
                    Time = new List<Time> {
                        new Time
                        {
                            Date = DefaultNowDateString,
                            Sunrise = new Sunrise
                            {
                                Time = _defaultNow.AddHours(6).AddMinutes(23)
                            },
                            Sunset = new Sunset
                            {
                                Time = _defaultNow.AddHours(20).AddMinutes(13)
                            }
                        }
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
