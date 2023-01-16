namespace NetatmoProxy.Configuration
{
    public class MetSunriseApiConfig
    {
        /// <summary>
        /// Location for where to get sunrise data
        /// </summary>
        public decimal Latitude { get; set; }
        /// <summary>
        /// Location for where to get sunrise data
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// Height above ellipsoide
        /// </summary>
        public decimal Height { get; set; }
    }
}
