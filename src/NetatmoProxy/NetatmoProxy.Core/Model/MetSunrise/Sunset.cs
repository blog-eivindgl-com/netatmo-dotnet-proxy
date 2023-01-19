using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Sunset
    {
        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }
}
