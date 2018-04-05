using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Durwella.UrlShortening.Web
{
    public class ConfigSettings: IConfigSettings
    {
        private readonly IConfiguration _config;
        public ConfigSettings(IConfiguration config)
        {
            _config = config;
        }

        public IConnectionStrings ConnectionStrings => new ConnectionStringsClass(_config);
        public IList<string> ProtectedPaths => new List<string>();
        public IList<HttpStatusCode> IgnoreErrorCodes => 
            _config["IgnoreErrorCodes"]
                ?.Split(',', ' ')
                .Select(s => (HttpStatusCode) Convert.ToInt32(s))
                .ToList() ?? new List<HttpStatusCode>();
        
        public bool ResolveUrls => _config.GetValue<bool>("ResolveUrls");
        public string RedirectUrl => _config["RedirectUrl"];
        public int PreferredHashLength => _config.GetValue("PreferredHashLength", 4);
        public IAuthSettings AuthSettings => new AuthSettingsClass(_config);

        class ConnectionStringsClass: IConnectionStrings
        {
            private readonly IConfiguration _config;
            public ConnectionStringsClass(IConfiguration config)
            {
                _config = config;
            }

            public string AzureStorage => _config["ConnectionStrings:AzureStorage"];
        }

        class AuthSettingsClass: IAuthSettings
        {
            private readonly IConfiguration _config;
            public AuthSettingsClass(IConfiguration config)
            {
                _config = config;
            }

            public string Secret => _config["Auth:Secret"];
            public string Issuer => _config["Auth:Issuer"];
            public string Audience => _config["Auth:Audience"];
            public int TokenExpirationMinutes => _config.GetValue("Auth:TokenExpirationMinutes", 60);
        }
    }
}
