using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Durwella.UrlShortening.Web.Services
{
    public class TokenService: ITokenService
    {
        private readonly IConfigSettings _config;
        public TokenService(IConfigSettings config)
        {
            _config = config;
        }

        public TokenResponse GenerateToken(string userName)
        {
            var now = DateTime.UtcNow;

            var claims = new[]
            {
                new Claim("user_name", userName),
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.AuthSettings.Secret));

            var jwt = new JwtSecurityToken(
                _config.AuthSettings.Issuer, 
                _config.AuthSettings.Audience, 
                claims, 
                now, 
                now.Add(TimeSpan.FromMinutes(_config.AuthSettings.TokenExpirationMinutes)), 
                new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new TokenResponse
            {
                AccessToken = encodedJwt,
                ExpiresIn = _config.AuthSettings.TokenExpirationMinutes * 60
            };
        }
    }
}
