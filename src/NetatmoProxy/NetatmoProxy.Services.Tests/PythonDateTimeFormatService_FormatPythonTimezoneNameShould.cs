using Moq;
using Shouldly;
using Xunit;

namespace NetatmoProxy.Services.Tests
{
    public class PythonDateTimeFormatService_FormatPythonTimezoneNameShould
    {
        private readonly Mock<INowService> _nowServiceMock;
        private readonly PythonDateTimeFormatService _service;

        public PythonDateTimeFormatService_FormatPythonTimezoneNameShould()
        {
            _nowServiceMock = new Mock<INowService>();
            _nowServiceMock.Setup(s => s.DateTimeNow).Returns(DateTime.Now);
            _nowServiceMock.Setup(s => s.DateTimeOffsetNow).Returns(DateTimeOffset.Now);
            _service = new PythonDateTimeFormatService(_nowServiceMock.Object);
        }

        [Theory]
        [InlineData(@"Europe/Oslo", "CET")]
        [InlineData(@"Europe/Tallinn", "EET")]
        [InlineData(@"America/Chicago", "CST")]
        public void Return_CET_For_EuropeOslo(string timezone, string expectedPythonTimeZoneName)
        {
            // Arrange
            string strfTime = "%Z";

            // Act
            string actualResult = _service.FormatPythonTimezoneName(timezone, strfTime);

            // Assert
            actualResult.ShouldBe(expectedPythonTimeZoneName);
        }

        [Theory]
        [InlineData("Europe/Stockholm")]
        [InlineData("Europe/London")]
        [InlineData("Europe/Berlin")]
        public void Throw_ArgumentOutOfRangeException_ForAllUnsupported_Timezones(string timezone)
        {
            // Arrange
            string strfTime = "%Z";
            Exception actualException = null;
            string expectedErrorMessage = @"Only timezones Europe/Oslo, Europe/Tallinn and America/Chicago are supported (Parameter 'timezone')";
            string actualResult = null;

            // Act
            try
            {
                actualResult = _service.FormatPythonTimezoneName(timezone, strfTime);
            }
            catch(Exception ex)
            {
                actualException = ex;
            }

            // Assert
            actualResult.ShouldBeNull("Expected no result");
            actualException.ShouldNotBeNull("An exception was thrown");
            actualException.ShouldBeOfType<ArgumentOutOfRangeException>("The expected type ArgumentOutOfRangeException was thrown");
            actualException.Message.ShouldBe(expectedErrorMessage, "The expected error message was used");
        }
    }
}
