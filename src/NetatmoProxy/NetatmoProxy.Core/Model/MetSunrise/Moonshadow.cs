using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Moonshadow
    {
        [JsonPropertyName("azimuth")]
        public string Azimuth { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("elevation")]
        public string Elevation { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }
}
