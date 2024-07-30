using HTApi.DTOs;
using HTAPI.Data;
using HTAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HTApi.Services
{

    public interface ITokenService
    {
        string GenerateJwtToken(string email, bool persist, int uuid, string identityUserId);
        User GetUserByJWT();
    }
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor? _http;

        private readonly IConfiguration _config;

        private readonly AppDbContext _db;

        public TokenService(IConfiguration config, IServiceProvider sp)
        {
            _config = config;
            _http = sp.GetService<IHttpContextAccessor>();
            _db = sp.GetRequiredService<AppDbContext>();
        }

        public string GenerateJwtToken(string email, bool persist, int uuid, string identityUserId)
        {
            string secretKey = _config["JWT:Secret"];
            string issuer = _config["JWT:ValidIssuer"];
            string audience = _config["JWT:ValidAudience"];

            double expiresInMinutes = persist ? 43800 : 60;

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.SerialNumber, uuid.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, identityUserId)

                }),
                Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        
        public User GetUserByJWT()
        {
            ClaimsIdentity claimsIdentity = _http.HttpContext.User.Identity as ClaimsIdentity;
            var claims = claimsIdentity?.Claims;

            if (claims == null) { throw new Exception("No claims found in jwt."); }

            UserClaimsDTO userClaimsDTO = new UserClaimsDTO
            {
                Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                Id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                UUID = claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value
            };

            Console.WriteLine(userClaimsDTO.Email);
            Console.WriteLine(userClaimsDTO.UUID);
            Console.WriteLine(userClaimsDTO.Id);

            User? requester = _db.Users.FirstOrDefault(u => (u.Id == userClaimsDTO.Id) &&
                                                            (u.UUID.ToString() == userClaimsDTO.UUID.ToString()) &&
                                                            (u.Email == userClaimsDTO.Email));

            if (requester == null) { throw new Exception("400 - This user does not exist."); }

            return requester;
        }
    }
}
