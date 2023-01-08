using Microsoft.AspNetCore.Mvc;
using NetatmoProxy.Services;

namespace NetatmoProxy.Controllers
{
    [Route("api/v2/{username}/integrations/time")]
    [ApiController]
    public class LocalTimeController : ControllerBase
    {
        private readonly IPythonDateTimeFormatService _dateTimeFormatService;

        public LocalTimeController(IPythonDateTimeFormatService dateTimeFormatService)
        {
            _dateTimeFormatService = dateTimeFormatService;
        }

        [HttpGet("strftime")]
        public string StrfTime([FromRoute]string username, [FromQuery(Name = "x-aio-key")]string adafruitApiKey, [FromQuery(Name = "tz")]string timezone, [FromQuery(Name = "fmt")]string format)
        {
            return _dateTimeFormatService.StrfTime(timezone, format);
        }
    }
}
