using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Durwella.UrlShortening
{
    public interface IConnectionStrings
    {
        string AzureStorage { get; }
    }

    public interface IAuthSettings
    {
        string Secret { get; }
        string Issuer { get; }
        string Audience { get; }
        int TokenExpirationMinutes { get; }
    }

    public interface IConfigSettings
    {
        IConnectionStrings ConnectionStrings { get; }
        IList<string> ProtectedPaths { get; }
        IList<HttpStatusCode> IgnoreErrorCodes { get; }
        bool ResolveUrls { get; }
        string RedirectUrl { get; }
        int PreferredHashLength { get; }
        IAuthSettings AuthSettings { get; }
    }
}
