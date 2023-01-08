using NetatmoProxy.Model.Netatmo;

namespace NetatmoProxy.Model
{
    public class TokenResponseWrapper
    {
        public TokenResponse TokenResponse { get; set; }
        public DateTimeOffset Expires {get; set; }
    }
}
