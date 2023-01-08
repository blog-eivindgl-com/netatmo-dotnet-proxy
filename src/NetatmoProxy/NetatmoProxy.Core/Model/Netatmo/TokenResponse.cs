using System.Text.Json.Serialization;

namespace NetatmoProxy.Model.Netatmo
{
    public class TokenResponse
    {
        public string[] Scope { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
