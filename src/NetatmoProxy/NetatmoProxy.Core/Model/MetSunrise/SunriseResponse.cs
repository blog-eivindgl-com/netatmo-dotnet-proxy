using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class SunriseResponse
    {
        [JsonPropertyName("location")]
        public Location Location { get; set; }
        [JsonPropertyName("meta")] 
        public Meta Meta { get; set; }
    }
}
