using Microsoft.Extensions.Caching.Memory;
using NetatmoProxy.Configuration;
using NetatmoProxy.Model.Netatmo;

namespace NetatmoProxy.Services
{
    public class NetatmoApiRestService : INetatmoApiService
    {
        public const string GetStationsDataCacheKey = "StationsData";
        public const string DefaultBaseUri = @"https://api.netatmo.com/api/";
        public const string GetStationsDataUri = "getstationsdata";
        private readonly NetatmoApiConfig _config;
        private readonly ILogger<NetatmoApiRestService> _logger;
        private readonly IAccessTokenService _tokenService;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memCache;

        public NetatmoApiRestService(NetatmoApiConfig config, ILogger<NetatmoApiRestService> logger, IAccessTokenService accessTokenService, HttpClientHandler httpClientHandler, IMemoryCache memCache)
        {
            _config = config;
            _logger = logger;
            _tokenService = accessTokenService;
            _httpClient = new HttpClient(httpClientHandler);
            _memCache = memCache;

            if (string.IsNullOrEmpty(_config?.BaseUri))
            {
                _httpClient.BaseAddress = new Uri(DefaultBaseUri);
            }
            else
            {
                _httpClient.BaseAddress = new Uri(_config.BaseUri);
            }
        }

        public async Task<GetStationsDataResponse?> GetStationsDataAsync(GetStationsDataRequest request)
        {
            return await _memCache.GetOrCreateAsync(
                GetStationsDataCacheKey,
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1); // Cache values for 1 minute so we don't spam netatmo api
                    return LoadStationsDataAsync(request);
                });
        }

        private async Task<GetStationsDataResponse?> LoadStationsDataAsync(GetStationsDataRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, GetStationsDataUri);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _tokenService.GetAccessTokenAsync());
            _logger.LogDebug(requestMessage.Headers.Authorization.ToString());
            var response = await _httpClient.SendAsync(requestMessage);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                _logger.LogError(await response.Content.ReadAsStringAsync());
                throw;
            }

            try
            {
                var getStationsDataResponse = await response.Content.ReadFromJsonAsync<GetStationsDataResponse>();

                if (getStationsDataResponse == null)
                {
                    _logger.LogDebug($"GetStationsDataResponse: {await response.Content.ReadAsStringAsync()}");
                }

                return getStationsDataResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing GetStationsDataResponse");
                _logger.LogError(await response.Content.ReadAsStringAsync());
                throw;
            }
        }
    }
}
