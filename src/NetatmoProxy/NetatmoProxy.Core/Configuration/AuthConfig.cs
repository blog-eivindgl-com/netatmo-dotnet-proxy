namespace NetatmoProxy.Configuration
{
    public class AuthConfig
    {
        public string BaseUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
        public string RefreshToken { get; set; }
    }
}
