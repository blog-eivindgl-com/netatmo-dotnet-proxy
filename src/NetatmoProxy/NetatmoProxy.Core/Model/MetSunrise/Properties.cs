using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Properties
    {
        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("sunrise")]
        public RiseSet? Sunrise { get; set; }

        [JsonPropertyName("sunset")]
        public RiseSet? Sunset { get; set; }

        [JsonPropertyName("solarnoon")]
        public Noon? Solarnoon { get; set; }

        [JsonPropertyName("solarmidnight")]
        public Noon? Solarmidnight { get; set; }

        [JsonPropertyName("moonrise")]
        public RiseSet? Moonrise { get; set; }

        [JsonPropertyName("moonset")]
        public RiseSet? Moonset { get; set; }

        [JsonPropertyName("high_moon")]
        public Noon? HigmMoon { get; set; }

        [JsonPropertyName("low_moon")]
        public Noon? LowMoon { get; set; }

        [JsonPropertyName("moonphase")]
        public decimal? Moonphase { get; set; }
    }
}
