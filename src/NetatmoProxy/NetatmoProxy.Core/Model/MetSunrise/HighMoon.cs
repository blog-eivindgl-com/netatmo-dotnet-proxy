using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class HighMoon
    {
        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("elevation")]
        public string Elevation { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }
}
