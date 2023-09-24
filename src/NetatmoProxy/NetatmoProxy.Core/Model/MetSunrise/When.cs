using System;
using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.MetSunrise
{
    public class When
    {
        [JsonPropertyName("interval")]
        public DateTime[] Interval { get; set; }
    }
}
