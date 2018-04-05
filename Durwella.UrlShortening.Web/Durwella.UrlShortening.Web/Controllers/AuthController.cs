using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Durwella.UrlShortening.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Durwella.UrlShortening.Web.Controllers
{
    [Route("[controller]")]
    public class AuthController: Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;

        public AuthController(IUserRepository userRepo, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
        }

        [HttpPost("cred")]
        public IActionResult Authorize([FromForm] CredentialAuthRequest request)
        {
            if (!_userRepo.Validate(request.UserName, request.Password))
            {
                return Unauthorized();
            }

            return Ok(_tokenService.GenerateToken(request.UserName));
        }

        [Authorize]
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            return Ok(true);
        }
    }
}
