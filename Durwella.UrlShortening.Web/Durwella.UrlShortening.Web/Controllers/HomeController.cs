using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Durwella.UrlShortening.Web.Controllers
{
    public class HomeController: Controller
    {
        private readonly IConfigSettings _config;
        public HomeController(IConfigSettings config)
        {
            _config = config;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            if (!string.IsNullOrWhiteSpace(_config.RedirectUrl))
            {
                return RedirectPermanent(_config.RedirectUrl);
            }

            return Admin();
        }

        [HttpGet("/admin")]
        public IActionResult Admin()
        {
            // https://github.com/aspnet/Mvc/issues/6875
            HttpContext.Request.Headers.Remove("If-Modified-Since");
            return File("~/index.html", "text/html");
        }
    }
}
