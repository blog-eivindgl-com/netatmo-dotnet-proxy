using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Moonphase
    {
        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
