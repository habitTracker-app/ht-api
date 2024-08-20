using HTApi.DTOs;
using HTApi.Models;
using HTApi.Models.Exceptions;
using HTAPI.Data;
using HTAPI.Models;
using Microsoft.AspNetCore.Authentication;
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
        Task BlockJWT();
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

        public bool IsJWTValid()
        {
            var token = _http.HttpContext.Request.Headers.Authorization.ToString();

            if(_db.TokenBlockList.Any(blocked => blocked.Token == token))
            {
                return false;
            }
            return true;
        }
        
        public User GetUserByJWT()
        {
            if (!IsJWTValid())
            {
                throw new BadRequestException("This JWT is blocked.", 403);
            }
            ClaimsIdentity claimsIdentity = _http.HttpContext.User.Identity as ClaimsIdentity;
            var claims = claimsIdentity?.Claims;

            if (claims == null) { throw new BadRequestException("No claims found in jwt.", 403); }

            UserClaimsDTO userClaimsDTO = new UserClaimsDTO
            {
                Id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                UUID = claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value
            };


            User? requester = _db.Users.FirstOrDefault(u => (u.Id == userClaimsDTO.Id) &&
                                                            (u.UUID.ToString() == userClaimsDTO.UUID.ToString()));

            if (requester == null) { throw new BadRequestException("This user does not exist.", 404); }

            return requester;
        }

        public async Task BlockJWT()
        {
            var token = _http.HttpContext.Request.Headers.Authorization;
            try
            {

                TokenBlockList blocked = new TokenBlockList()
                {
                    InactivatedAt = DateTime.UtcNow,
                    Token = token
                };
                _db.TokenBlockList.Add(blocked);

                await _db.SaveChangesAsync();
            }
            catch (BadRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(token, ex);
            }
        }
    }
}
