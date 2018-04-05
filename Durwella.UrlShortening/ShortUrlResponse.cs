using System;
using System.Collections.Generic;
using System.Text;

namespace Durwella.UrlShortening
{
    public class ShortUrlResponse
    {
        public string Shortened { get; set; }

        public ShortUrlResponse()
        {
        }

        public ShortUrlResponse(string baseUri, string key)
        {
            Shortened = new Uri(new Uri(baseUri), key).ToString();
        }
    }
}
