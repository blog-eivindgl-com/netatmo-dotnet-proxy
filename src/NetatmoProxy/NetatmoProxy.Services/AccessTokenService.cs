using Microsoft.Extensions.Caching.Memory;
using NetatmoProxy.Configuration;
using NetatmoProxy.Model;
using NetatmoProxy.Model.Netatmo;

namespace NetatmoProxy.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        public const string TokenResponseCacheKey = "TokenResponse";
        public const string BaseUri = @"https://api.netatmo.net/oauth2/token";
        private readonly AuthConfig _config;
        private readonly ILogger<AccessTokenService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memCache;

        public AccessTokenService(AuthConfig config, ILogger<AccessTokenService> logger, HttpClientHandler httpClientHandler, IMemoryCache memCache)
        {
            _config = config;
            _logger = logger;
            _httpClient = new HttpClient(httpClientHandler);
            _memCache = memCache;

            if (string.IsNullOrEmpty(_config?.BaseUri))
            {
                _httpClient.BaseAddress = new Uri(BaseUri);
            }
            else
            {
                _httpClient.BaseAddress = new Uri(config.BaseUri);
            }
        }

        public async Task<string> GetAccessTokenAsync(bool forceFullLoad = false)
        {
            var now = DateTimeOffset.Now;  // Time before request is sent
            TokenResponseWrapper? tokenResponseWrapper = _memCache.Get<TokenResponseWrapper>(TokenResponseCacheKey);
            TokenResponse? tokenResponse = null;

            if (!forceFullLoad && tokenResponseWrapper != null)
            {
                // Refresh token if it expires in less than 10 seconds
                if (tokenResponseWrapper.Expires <= now.AddSeconds(10))
                {
                    tokenResponse = await RefreshTokenAsync(tokenResponseWrapper.TokenResponse.RefreshToken);
                }
                else
                {
                    return tokenResponseWrapper.TokenResponse.AccessToken;
                }
            }
            else
            {
                tokenResponse = await GetTokenFirstTimeAsync();
            }

            if (tokenResponse != null)
            {
                _memCache.Set(
                    key: TokenResponseCacheKey, 
                    value: new TokenResponseWrapper
                    {
                        TokenResponse = tokenResponse,
                        Expires = now.AddSeconds(tokenResponse.ExpiresIn)
                    },
                    options: new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1))  // Extende cached value one day
                    );
                return tokenResponse.AccessToken;
            }
            else
            {
                // Make sure we do a first time request next time
                _memCache.Remove(TokenResponseCacheKey);
            }

            return "invalid-token";
        }

        private async Task<TokenResponse?> GetTokenFirstTimeAsync()
        {
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
            _logger.LogDebug($"First time token, Uri: {_httpClient.BaseAddress}, Content: {await request.Content.ReadAsStringAsync()}");
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

            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }

        private async Task<TokenResponse?> RefreshTokenAsync(string refreshToken)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new FormUrlEncodedContent(
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("client_id", _config.ClientId),
                    new KeyValuePair<string, string>("client_secret", _config.ClientSecret)
                });
            _logger.LogDebug($"Refresh token, Uri: {_httpClient.BaseAddress}, Content: {await request.Content.ReadAsStringAsync()}");
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

            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }
    }
}
