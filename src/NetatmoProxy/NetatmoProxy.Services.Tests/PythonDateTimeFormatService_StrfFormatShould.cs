using Moq;
using Shouldly;
using System.Globalization;
using Xunit;

namespace NetatmoProxy.Services.Tests
{
    public class PythonDateTimeFormatService_StrfFormatShould
    {
        private readonly DateTimeOffset _mockedNow = DateTimeOffset.ParseExact("2023-02-15 14:15:16.123 +01:00", "yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.CurrentCulture);
        private readonly Mock<INowService> _nowServiceMock;
        private readonly PythonDateTimeFormatService _service;

        public PythonDateTimeFormatService_StrfFormatShould()
        {
            _nowServiceMock = new Mock<INowService>();
            _nowServiceMock.Setup(s => s.DateTimeNow).Returns(_mockedNow.DateTime);
            _nowServiceMock.Setup(s => s.DateTimeOffsetNow).Returns(_mockedNow);
            _nowServiceMock.Setup(s => s.UtcNow).Returns(_mockedNow.ToUniversalTime());
            _service = new PythonDateTimeFormatService(_nowServiceMock.Object);
        }

        [Theory]
        [InlineData("%Y-%m-%d %H:%M:%S.%L", "2023-02-15 14:15:16.123")]
        [InlineData("%Y-%m-%d %H:%M:%S.%L %z", "2023-02-15 14:15:16.123 +0100")]
        [InlineData("%Y-%m-%d %H:%M:%S.%L %j %z", "2023-02-15 14:15:16.123 046 +0100")]
        [InlineData("%Y-%m-%d %H:%M:%S.%L %j %u %z", "2023-02-15 14:15:16.123 046 3 +0100")]
        [InlineData("%Y-%m-%d %H:%M:%S.%L %j %u %z %Z", "2023-02-15 14:15:16.123 046 3 +0100 CET")]
        public void SupportKnownDateTimeFormatForPython(string format, string expectedOutput)
        {
            // Arrange
            string timezone = @"Europe/Oslo";

            // Act
            string actualOutput = _service.StrfTime(timezone, format);

            // Assert
            actualOutput.ShouldBe(expectedOutput, "Output as expected");
        }
    }
}
