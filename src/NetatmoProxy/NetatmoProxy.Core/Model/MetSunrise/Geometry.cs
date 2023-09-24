using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Geometry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("coordinates")]
        public decimal[] Coordinates { get; set; }
    }
}
