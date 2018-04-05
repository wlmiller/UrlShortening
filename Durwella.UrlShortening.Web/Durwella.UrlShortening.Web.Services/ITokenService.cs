using System;
using System.Collections.Generic;
using System.Text;

namespace Durwella.UrlShortening.Web.Services
{
    public interface ITokenService
    {
        TokenResponse GenerateToken(string userName);
    }
}
