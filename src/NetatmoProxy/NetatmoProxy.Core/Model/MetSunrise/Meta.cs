using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Meta
    {
        [JsonPropertyName("licenseurl")]
        public string Licenseurl { get; set; }
    }
}
