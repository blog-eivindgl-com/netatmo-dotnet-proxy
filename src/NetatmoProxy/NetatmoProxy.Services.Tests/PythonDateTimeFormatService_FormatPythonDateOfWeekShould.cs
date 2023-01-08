using Moq;
using Shouldly;
using System.Globalization;
using Xunit;

namespace NetatmoProxy.Services.Tests
{
    public class PythonDateTimeFormatService_FormatPythonDateOfWeekShould
    {
        private readonly Mock<INowService> _nowServiceMock;
        private readonly PythonDateTimeFormatService _service;

        public PythonDateTimeFormatService_FormatPythonDateOfWeekShould()
        {
            _nowServiceMock = new Mock<INowService>();
            _nowServiceMock.Setup(s => s.DateTimeNow).Returns(DateTime.Now);
            _nowServiceMock.Setup(s => s.DateTimeOffsetNow).Returns(DateTimeOffset.Now);
            _service = new PythonDateTimeFormatService(_nowServiceMock.Object);
        }

        [Theory]
        [InlineData("2023-01-02 12:00:00", "1")]
        [InlineData("2023-01-03 12:00:00", "2")]
        [InlineData("2023-01-04 12:00:00", "3")]
        [InlineData("2023-01-05 12:00:00", "4")]
        [InlineData("2023-01-06 12:00:00", "5")]
        [InlineData("2023-01-07 12:00:00", "6")]
        [InlineData("2023-01-08 12:00:00", "7")]
        public void SupportAllWeekdays(string mockedNow, string expectedDayOfWeek)
        {
            // Arrange
            DateTimeOffset mockedNowValue = DateTimeOffset.ParseExact(mockedNow, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentUICulture);
            string dayOfWeekFormat = "%u";

            // Act
            string actualDayOfWeek = _service.FormatPythonDayOfWeek(mockedNowValue, dayOfWeekFormat);

            // Assert
            actualDayOfWeek.ShouldBe(expectedDayOfWeek, "DayOfWeek formatted to the expected value");

        }
    }
}
