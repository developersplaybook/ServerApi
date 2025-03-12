using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServerAPI.Models;
using ServerAPI.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        public const string SessionUserLoggedIn = "UserLoggedIn";

        private readonly IConfiguration _configuration;
        private readonly IKeyService _keyService;


        public AuthorizationController(IConfiguration configuration, IKeyService keyService)
        {
            _configuration = configuration;
            _keyService = keyService;
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            bool isLoggedIn = HttpContext.Session.GetValue<bool>(HttpContext, SessionUserLoggedIn);

            if (isLoggedIn)
            {
                HttpContext.Session.SetValue(HttpContext, SessionUserLoggedIn, false);
                return Ok(new { success = true, text = "userLoggedOut" });
            }

            return Ok(new { success = true, text = "userAlreadyLoggedOut" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var storedPassword = _configuration["Password"];

                if (model.Password == storedPassword)
                {
                    var key = _keyService.GetKey();
                    var keyBytes = Convert.FromBase64String(key);
                    var symmetricKey = new SymmetricSecurityKey(keyBytes);
                    var creds = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

                    var claims = new[]
                    {
                         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                    };

                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(30),
                        signingCredentials: creds);

                    HttpContext.Session.SetValue(HttpContext, SessionUserLoggedIn, true);
                    
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token)
                    });
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }

            return Unauthorized(new { message = "ModelState invalid" });
        }

        public class LoginModel
        {
            public string Password { get; set; }
        }
    }
}
