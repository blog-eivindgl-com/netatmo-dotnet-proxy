using System.Globalization;

namespace NetatmoProxy.Services
{
    public class PythonDateTimeFormatService : IPythonDateTimeFormatService
    {
        private readonly INowService _nowService;

        public PythonDateTimeFormatService(INowService nowService)
        {
            _nowService = nowService;
        }

        public string StrfTime(string timezone, string format)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(_nowService.UtcNow.DateTime, TimeZoneInfo.FindSystemTimeZoneById(NetTimeZoneInfoFromPythonTimeZone(timezone)));
            string strfTime = FormatPythonDayOfYear(now, format);
            strfTime = FormatPythonDayOfWeek(now, strfTime);
            strfTime = FormatPythonTimezoneName(timezone, strfTime);
            strfTime = FormatPythonOffset(now, strfTime);
            strfTime = now.ToString(ToCompatibleFormat(strfTime));


            return strfTime;
        }

        public string NetTimeZoneInfoFromPythonTimeZone(string timezone)
        {
            switch(timezone)
            {
                case @"Europe/Oslo":
                    return "W. Europe Standard Time";
                default:
                    // TODO: Support other timezones than Europe/Oslo
                    throw new ArgumentOutOfRangeException(nameof(timezone), "Only timezone Europe/Oslo is supported");
            }
        }

        public string FormatPythonOffset(DateTimeOffset now, string strfTime)
        {
            if (strfTime.Contains("%z"))
            {
                string offset = now.ToString("zzz");
                offset = offset.Replace(":", "");
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
                    case "Europe/Oslo":
                        value = "CET";
                        break;
                    default:
                        // TODO: Support other timezones than Europe/Oslo
                        throw new ArgumentOutOfRangeException(nameof(timezone), "Only timezone Europe/Oslo is supported");
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
