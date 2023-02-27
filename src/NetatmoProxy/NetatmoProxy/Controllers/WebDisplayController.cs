using Microsoft.AspNetCore.Mvc;

namespace NetatmoProxy.Controllers
{
    [Route("display")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WebDisplayController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
