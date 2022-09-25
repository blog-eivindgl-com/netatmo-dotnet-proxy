using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.Netatmo
{
    public class Device
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("date_setup")]
        public int DateSetup { get; set; }
        [JsonPropertyName("last_setup")]
        public int LastSetup { get; set; }
        public string Type { get; set; }
        [JsonPropertyName("last_status_store")]
        public int LastStatusStore { get; set; }
        [JsonPropertyName("module_name")]
        public string ModuleName { get; set; }
        public int Firmware { get; set; }
        [JsonPropertyName("wifi_status")]
        public int WifiStatus { get; set; }
        public bool Reachable { get; set; }
        [JsonPropertyName("co2_calibrating")]
        public bool CO2Calibrating { get; set; }
        [JsonPropertyName("data_type")]
        public string[] DataType { get; set; }
        public Place Place { get; set; }
        [JsonPropertyName("station_name")]
        public string StationName { get; set; }
        [JsonPropertyName("home_id")]
        public string HomeId { get; set; }
        [JsonPropertyName("home_name")]
        public string HomeName { get; set; }
        [JsonPropertyName("dashboard_data")]
        public DashboardData DashboardData { get; set; }
        public IEnumerable<Module> Modules { get; set; }
    }
}
