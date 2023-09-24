using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class RiseSet
    {
        [JsonPropertyName("azimuth")]
        public decimal Azimuth { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }
}
