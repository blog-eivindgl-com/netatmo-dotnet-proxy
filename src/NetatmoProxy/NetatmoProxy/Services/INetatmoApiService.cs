using NetatmoProxy.Model.Netatmo;

namespace NetatmoProxy.Services
{
    public interface INetatmoApiService
    {
        Task<GetStationsDataResponse> GetStationsDataAsync(GetStationsDataRequest request);
    }
}
