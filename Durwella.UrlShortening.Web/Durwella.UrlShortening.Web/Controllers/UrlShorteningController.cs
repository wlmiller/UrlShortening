using System;
using System.Net;
using System.Threading.Tasks;
using Durwella.UrlShortening.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Durwella.UrlShortening.Web.Controllers
{
    public class UrlShorteningController: Controller
    {
        private readonly IUrlShorteningService _service;

        public UrlShorteningController(IUrlShorteningService service)
        {
            _service = service;
        }

        [HttpGet("/{key}")]
        public async Task<IActionResult> Get(string key)
        {
            try
            {
                var redirectUrl = await _service.GetRedirectUrl(key);

                return Redirect(redirectUrl);
            }
            catch (ShortUrlNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody] ShortUrlRequest request)
        {
            try
            {
                var key = await _service.Shorten(request);
                var baseUri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                return Ok(new ShortUrlResponse(baseUri, key));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UriFormatException)
            {
                return BadRequest("The specific url is invalid");
            }
            catch (WebException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
