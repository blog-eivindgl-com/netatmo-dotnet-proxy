using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.Netatmo
{
    public class DashboardData
    {
        [JsonPropertyName("time_utc")]
        public int TimeUtc { get; set; }
        public decimal Temperature { get; set; }
        public int CO2 { get; set; }
        public int Humidity { get; set; }
        public int Noice { get; set; }
        public decimal Pressure { get; set; }
        public decimal AbsolutePressure { get; set; }
        [JsonPropertyName("min_temp")]
        public decimal MinTemp { get; set; }
        [JsonPropertyName("max_temp")]
        public decimal MaxTemp { get; set; }
        [JsonPropertyName("date_max_temp")]
        public int DateMaxTemp { get; set; }
        [JsonPropertyName("date_min_temp")]
        public int DateMinTemp { get; set; }
        [JsonPropertyName("temp_trend")]
        public string TempTrend { get; set; }
        [JsonPropertyName("pressure_trend")]
        public string PressureTrend { get; set; }
        public int WindStrength { get; set; }
        public int WindAngle { get; set; }
        public int GustStrength { get; set; }
        public int GustAngle { get; set; }
        [JsonPropertyName("max_wind_str")]
        public int MaxWindStrength { get; set; }
        [JsonPropertyName("max_wind_angle")]
        public int MaxWindAngle { get; set; }
        [JsonPropertyName("date_max_wind_str")]
        public int DateMaxWind { get; set; }
    }
}
