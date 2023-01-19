using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Moonposition
    {
        [JsonPropertyName("azimuth")]
        public string Azimuth { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("elevation")]
        public string Elevation { get; set; }

        [JsonPropertyName("phase")]
        public string Phase { get; set; }

        [JsonPropertyName("range")]
        public string Range { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }
}
