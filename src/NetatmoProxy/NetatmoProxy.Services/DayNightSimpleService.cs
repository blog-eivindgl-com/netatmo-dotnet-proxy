namespace NetatmoProxy.Services
{
    public class DayNightSimpleService : IDayNightService
    {
        public Task<string> IsSunOrMoonAsync()
        {
            return Task.FromResult((DateTime.Now.Hour >= 20 && DateTime.Now.Hour <= 8) ? "moon" : "sun");
        }
    }
}
