using Microsoft.AspNetCore.Mvc;

namespace NetatmoProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TraceController : ControllerBase
    {
        private readonly ILogger<TraceController> _logger;

        public TraceController(ILogger<TraceController> logger) 
        { 
            _logger = logger;
        }

        [HttpPost("Information")]
        public IActionResult Information([FromBody] string text)
        {
            _logger.LogInformation(text);
            return Ok();
        }

        [HttpPost("Error")]
        public IActionResult Error([FromBody] string error) 
        {
            _logger.LogError(error);
            return Ok();
        }
    }
}
