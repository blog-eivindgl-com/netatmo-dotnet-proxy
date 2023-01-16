using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Location
    {
        [JsonPropertyName("height")]
        public string Height { get; set; }

        [JsonPropertyName("latitude")]
        public string Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string Longitude { get; set; }

        [JsonPropertyName("time")]
        public List<Time> Time { get; set; }
    }
}
