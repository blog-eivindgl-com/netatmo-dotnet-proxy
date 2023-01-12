namespace NetatmoProxy.Services
{
    public interface INowService
    {
        DateTime DateTimeNow { get; }
        DateTimeOffset DateTimeOffsetNow { get; }
        DateTime UtcNow { get; }
    }
}
