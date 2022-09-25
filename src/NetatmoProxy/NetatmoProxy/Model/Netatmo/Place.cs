namespace NetatmoProxy.Model.Netatmo
{
    public class Place
    {
        public int Altitude { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Timezone { get; set; }
        public decimal[] Location { get; set; }
    }
}
