namespace NetatmoProxy.Model.Netatmo
{
    public class GetStationsDataResponse : NetatmoResponse
    {
        public GetStationsResponseBody Body { get; set; }
    }

    public class GetStationsResponseBody
    {
        public IEnumerable<Device> Devices { get; set; }
        public User User { get; set; }
    }
}
