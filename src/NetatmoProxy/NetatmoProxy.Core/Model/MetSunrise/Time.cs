using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class Time
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("high_moon")]
        public HighMoon HighMoon { get; set; }

        [JsonPropertyName("low_moon")]
        public LowMoon LowMoon { get; set; }

        [JsonPropertyName("moonphase")]
        public Moonphase Moonphase { get; set; }

        [JsonPropertyName("moonposition")]
        public Moonposition Moonposition { get; set; }

        [JsonPropertyName("moonrise")]
        public Moonrise Moonrise { get; set; }

        [JsonPropertyName("moonset")]
        public Moonset Moonset { get; set; }

        [JsonPropertyName("moonshadow")]
        public Moonshadow Moonshadow { get; set; }

        [JsonPropertyName("solarmidnight")]
        public Solarmidnight Solarmidnight { get; set; }

        [JsonPropertyName("solarnoon")]
        public Solarnoon Solarnoon { get; set; }

        [JsonPropertyName("sunrise")]
        public Sunrise Sunrise { get; set; }

        [JsonPropertyName("sunset")]
        public Sunset Sunset { get; set; }
    }
}
