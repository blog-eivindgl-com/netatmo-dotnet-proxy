using Xunit;

namespace NetatmoProxy.Services.Tests
{
    public class TimeZoneInfo_SerializeShould
    {
        [Theory]
        [InlineData]
        public void SerializeIntoExpectedString()
        {
            string timeZoneInfo = TimeZoneInfo.Local.ToSerializedString();
        }
    }
}
