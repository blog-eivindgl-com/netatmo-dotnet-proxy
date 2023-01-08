namespace NetatmoProxy.Services
{
    public interface IPythonDateTimeFormatService
    {
        string StrfTime(string timezone, string format);
    }
}
