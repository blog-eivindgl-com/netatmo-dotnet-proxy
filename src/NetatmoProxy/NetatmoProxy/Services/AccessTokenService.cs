using NetatmoProxy.Configuration;
using NetatmoProxy.Model.Netatmo;

namespace NetatmoProxy.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        public const string BaseUri = @"https://api.netatmo.net/oauth2/token";
        private readonly AuthConfig _config;
        private readonly ILogger<AccessTokenService> _logger;
        private readonly HttpClient _httpClient;

        public AccessTokenService(AuthConfig config, ILogger<AccessTokenService> logger, HttpClientHandler httpClientHandler)
        {
            _config = config;
            _logger = logger;
            _httpClient = new HttpClient(httpClientHandler);

            if (string.IsNullOrEmpty(_config?.BaseUri))
            {
                _httpClient.BaseAddress = new Uri(BaseUri);
            }
            else
            {
                _httpClient.BaseAddress = new Uri(config.BaseUri);
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // TODO: Implement a token cache and refresh access token only when necessary
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("grant_type", _config.GrantType),
                    new KeyValuePair<string, string>("client_id", _config.ClientId),
                    new KeyValuePair<string, string>("client_secret", _config.ClientSecret),
                    new KeyValuePair<string, string>("username", _config.Username),
                    new KeyValuePair<string, string>("password", _config.Password)
                });
            _logger.LogDebug($"Uri: {_httpClient.BaseAddress}, Content: {await request.Content.ReadAsStringAsync()}");
            var response = await _httpClient.SendAsync(request);
            
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                _logger.LogError(await response.Content.ReadAsStringAsync());
                throw;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

            return tokenResponse.AccessToken;
        }
    }
}
