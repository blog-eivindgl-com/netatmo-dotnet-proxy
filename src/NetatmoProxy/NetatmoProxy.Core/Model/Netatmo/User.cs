namespace NetatmoProxy.Model.Netatmo
{
    public class User
    {
        public string Mail { get; set; }
        public UserAdministrative Administrative { get; set; }
    }

    public class UserAdministrative
    {
        public string Lang { get; set; }
        public string RegLocale { get; set; }
        public string Country { get; set; }
        public int Unit { get; set; }
        public int Windunit { get; set; }
        public int Pressureunit { get; set; }
        public int FeelLikeAlgo { get; set; }
    }
}
