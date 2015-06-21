﻿using ServiceStack;

namespace Durwella.UrlShortening.Web.ServiceModel
{
    [Api("URL Shortening")]
    [Route("/shorten", "POST", Summary = "Create a short URL that will redirect to the given URL.", Notes = "The given URL should be well-formatted and live or reachable.")]
    public class ShortUrlRequest : IReturn<ShortUrlResponse>
    {
        public string Url { get; set; }
    }

    [Api("URL Shortening")]
    [Route("/shorten-custom", "POST", Summary = "Create a customized short URL that will redirect to the given URL.", Notes = "The given URL should be well-formatted and live or reachable.")]
    public class CustomShortUrlRequest : ShortUrlRequest
    {
        public string CustomPath { get; set; }
    }
}
