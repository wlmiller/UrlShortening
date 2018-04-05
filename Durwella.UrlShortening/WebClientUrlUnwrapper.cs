using System;
using System.Collections.Generic;
using System.Net;

namespace Durwella.UrlShortening
{
    public class WebClientUrlUnwrapper : IUrlUnwrapper
    {
        private readonly IConfigSettings _config;
        public WebClientUrlUnwrapper(IConfigSettings config)
        {
            _config = config;
        }

        /// <summary>
        /// Set IgnoreErrorCodes to list of HTTP Status Codes that 
        /// should be ignored when testing the URL. For example,
        /// if you want to allow URLs to secure resources to be shortened 
        /// you can set IgnoreErrorCodes = new[] { HttpStatusCode.Unauthorized }
        /// </summary>
        private IList<HttpStatusCode> IgnoreErrorCodes => _config.IgnoreErrorCodes;

        private bool ResolveUrls => _config.ResolveUrls;

        public string GetDirectUrl(string url)
        {
            if (!ResolveUrls) return url;

            WebRequest request;
            try
            {
                request = WebRequest.Create(url);
            }
            catch (UriFormatException)
            {
                request = WebRequest.Create($"http://{url}");
            }
            
            try
            {
                using (var response = request.GetResponse())
                    return response.ResponseUri.ToString();
            }
            catch (WebException ex)
            {
                if (!(ex.Response is HttpWebResponse httpResponse) || 
                    IgnoreErrorCodes == null ||
                    !IgnoreErrorCodes.Contains(httpResponse.StatusCode))
                    throw;
                return url;
            }
        }
    }
}
