using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Noon
    {
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("disc_centre_elevation")]
        public decimal Elevation { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }
}
