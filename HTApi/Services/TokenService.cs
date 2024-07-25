using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HTApi.Services
{

    public interface ITokenService
    {
        string GenerateJwtToken(string email, bool persist, int uuid, string identityUserId);
    }
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
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
    }
}
