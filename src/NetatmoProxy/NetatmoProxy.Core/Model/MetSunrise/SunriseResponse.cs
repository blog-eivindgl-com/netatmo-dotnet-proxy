using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class SunriseResponse
    {
        [JsonPropertyName("copyright")]
        public string Copyright { get; set; }
        [JsonPropertyName("licenseURL")]
        public string LicenseUrl { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; }
        [JsonPropertyName("when")] 
        public When When { get; set; }
        [JsonPropertyName("properties")]
        public Properties Properties { get; set; }
    }
}
