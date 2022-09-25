namespace NetatmoProxy.Model.Netatmo
{
    public class GetStationsDataRequest
    {
        public string? DeviceId { get; set; }
        public bool? GetFavorites { get; set; }
    }
}
