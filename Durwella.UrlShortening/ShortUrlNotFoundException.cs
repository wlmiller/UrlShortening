using System;

namespace Durwella.UrlShortening
{
    public class ShortUrlNotFoundException: ApplicationException
    {
        public ShortUrlNotFoundException(string message) : base(message)
        {
        }
    }
}
