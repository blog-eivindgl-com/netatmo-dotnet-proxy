using Prometheus;

namespace NetatmoProxy.Services
{
    public class PythonDateTimeFormatService : IPythonDateTimeFormatService
    {
        public const string SupportedTimezonesErrorMessage = @"Only timezones Europe/Oslo, Europe/Tallinn and America/Chicago are supported";
        public const string AdafruitTimezoneEuropeOslo = @"Europe/Oslo";
        public const string AdafruitTimezoneEuropeTallinn = @"Europe/Tallinn";
        public const string AdafruitTimezoneAmericaChicago = @"America/Chicago";
        public const string PythonTimezoneCET = "CET";
        public const string PythonTimezoneEET = "EET";
        public const string PythonTimezoneCST = "CST";
        public const string NetTimezoneWestEuropeStandardTime = "W. Europe Standard Time";
        public const string NetTimezoneEastEuropeStandardTime = "E. Europe Standard Time";
        public const string NetTimezoneCentralStandardTime = "Central Standard Time";
        private static readonly Counter StrfTimeCalls = Metrics.CreateCounter($"{nameof(PythonDateTimeFormatService).ToLower()}_{nameof(StrfTime).ToLower()}calls_total", "Number of times StrfTime has been called.");

        private readonly INowService _nowService;

        public PythonDateTimeFormatService(INowService nowService)
        {
            _nowService = nowService;
        }

        public string StrfTime(string timezone, string format)
        {
            StrfTimeCalls.Inc();
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(NetTimeZoneInfoFromPythonTimeZone(timezone));
            var now = TimeZoneInfo.ConvertTimeFromUtc(_nowService.UtcNow, tzi);
            string strfTime = FormatPythonDayOfYear(now, format);
            strfTime = FormatPythonDayOfWeek(now, strfTime);
            strfTime = FormatPythonTimezoneName(timezone, strfTime);
            strfTime = FormatPythonOffset(now, tzi, strfTime);
            strfTime = now.ToString(ToCompatibleFormat(strfTime));


            return strfTime;
        }

        public string NetTimeZoneInfoFromPythonTimeZone(string timezone)
        {
            switch(timezone)
            {
                case AdafruitTimezoneEuropeOslo:
                    return NetTimezoneWestEuropeStandardTime;
                case AdafruitTimezoneEuropeTallinn:
                    return NetTimezoneEastEuropeStandardTime;
                case AdafruitTimezoneAmericaChicago:
                    return NetTimezoneCentralStandardTime;
                default:
                    // TODO: Support other timezones
                    throw new ArgumentOutOfRangeException(nameof(timezone), SupportedTimezonesErrorMessage);
            }
        }

        public string FormatPythonOffset(DateTimeOffset now, TimeZoneInfo tzi, string strfTime)
        {
            if (strfTime.Contains("%z"))
            {
                string offset = tzi.GetUtcOffset(now).ToString("hhmm");
                offset = tzi.BaseUtcOffset < TimeSpan.Zero ? $"-{offset}" : $"+{offset}";
                return strfTime.Replace("%z", offset);
            }

            return strfTime;
        }

        public string FormatPythonTimezoneName(string timezone, string strfTime)
        {
            if (strfTime.Contains("%Z"))
            {
                string value;

                switch(timezone)
                {
                    case AdafruitTimezoneEuropeOslo:
                        value = PythonTimezoneCET;
                        break;
                    case AdafruitTimezoneEuropeTallinn:
                        value = PythonTimezoneEET;
                        break;
                    case AdafruitTimezoneAmericaChicago:
                        value = PythonTimezoneCST;
                        break;
                    default:
                        // TODO: Support other timezones
                        throw new ArgumentOutOfRangeException(nameof(timezone), SupportedTimezonesErrorMessage);
                }

                return strfTime.Replace("%Z", value);
            }

            return strfTime;
        }

        public string FormatPythonDayOfWeek(DateTimeOffset now, string strfTime)
        {
            int dayOfWeek = now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek;
            return strfTime.Replace("%u", dayOfWeek.ToString());
        }

        public string FormatPythonDayOfYear(DateTimeOffset now, string strfTime)
        {
            return strfTime.Replace("%j", now.DayOfYear.ToString("000"));
        }

        public string ToCompatibleFormat(string pythonFormat)
        {
            /* Sample format used by PyPortal:
             * %Y-%m-%d %H:%M:%S.%L %j %u %z %Z 
             */

            string format = pythonFormat;
            format = format.Replace("%Y", "yyyy");
            format = format.Replace("%m", "MM");
            format = format.Replace("%d", "dd");
            format = format.Replace("%H", "HH");
            format = format.Replace("%M", "mm");
            format = format.Replace("%S", "ss");
            format = format.Replace("%L", "fff");

            return format;
        }
    }
}
