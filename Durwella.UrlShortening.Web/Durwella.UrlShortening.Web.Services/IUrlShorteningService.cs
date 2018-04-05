using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Durwella.UrlShortening.Web.Services
{
    public interface IUrlShorteningService
    {
        Task<string> Shorten(ShortUrlRequest shortUrlRequest);
        Task<string> GetRedirectUrl(string key);
    }
}
