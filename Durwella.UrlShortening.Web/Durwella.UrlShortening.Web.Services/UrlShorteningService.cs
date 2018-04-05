using System;
using System.Threading.Tasks;
using Durwella.UrlShortening.Web.ServiceInterface;

namespace Durwella.UrlShortening.Web.Services
{
    public class UrlShorteningService : IUrlShorteningService
    {
        private readonly IAliasRepository _aliasRepository;
        private readonly IProtectedPathList _protectedPathList;
        private readonly IHashScheme _hashScheme;
        private readonly IUrlUnwrapper _urlUnwrapper;

        public UrlShorteningService(IAliasRepository aliasRepository, IProtectedPathList protectedPathList,
            IHashScheme hashScheme, IUrlUnwrapper urlUnwrapper)
        {
            _aliasRepository = aliasRepository;
            _protectedPathList = protectedPathList;
            _hashScheme = hashScheme;
            _urlUnwrapper = urlUnwrapper;
        }

        public Task<string> Shorten(ShortUrlRequest shortUrlRequest)
        {
            return String.IsNullOrWhiteSpace(shortUrlRequest.CustomPath) ?
                MakeShortUrl(shortener => shortener.Shorten(shortUrlRequest.Url)) : 
                MakeShortUrl(shortener => shortener.ShortenWithCustomHash(shortUrlRequest.Url, shortUrlRequest.CustomPath));
        }
        
        public async Task<string> GetRedirectUrl(string key)
        {
            if (!(await _aliasRepository.ContainsKey(key)))
                throw new ShortUrlNotFoundException($"Short URL '{key}' does not exist");

            return await _aliasRepository.GetValue(key);
        }

        private async Task<string> MakeShortUrl(Func<UrlShortener, Task<string>> shorten)
        {
            var urlShortener = new UrlShortener(_aliasRepository, _hashScheme, _urlUnwrapper)
            {
                ProtectedPaths = _protectedPathList.ProtectedPaths
            };

            var shortened = await shorten(urlShortener);

            return shortened;
        }
    }
}
