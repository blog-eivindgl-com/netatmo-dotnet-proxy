using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.Netatmo
{
    public class Module
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        public string Type { get; set; }
        [JsonPropertyName("module_name")]
        public string ModuleName { get; set; }
        [JsonPropertyName("last_setup")]
        public int LastSetup { get; set; }
        [JsonPropertyName("data_type")]
        public string[] DataType { get; set; }
        [JsonPropertyName("battery_percent")]
        public int BatteryPercent { get; set; }
        public bool Reachable { get; set; }
        public int Firmware { get; set; }
        [JsonPropertyName("last_message")]
        public int LastMessage { get; set; }
        [JsonPropertyName("last_seen")]
        public int LastSeen { get; set; }
        [JsonPropertyName("rf_status")]
        public int RfStatus { get; set; }
        [JsonPropertyName("battery_vp")]
        public int BatteryVp { get; set; }
        [JsonPropertyName("dashboard_data")]
        public DashboardData DashboardData { get; set; }
    }
}
