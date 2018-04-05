using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Durwella.UrlShortening
{
    public class TokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; } = "Bearer";
    }
}
