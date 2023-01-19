using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Sunrise
    {
        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }
}
