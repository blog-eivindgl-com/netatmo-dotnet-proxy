namespace NetatmoProxy.Services
{
    public class NowService : INowService
    {
        public DateTime DateTimeNow => DateTime.Now;

        public DateTimeOffset DateTimeOffsetNow => DateTimeOffset.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
