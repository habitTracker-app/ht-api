using HTApi.DTOs;
using HTApi.Models;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.ActionModels;
using HTAPI.Models.DemographicData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<User> _um;
        private readonly SignInManager<User> _sm;
        private readonly AppDbContext _db;
        private readonly ITokenService _jwt;
        private readonly IValidationService _valid;
        private readonly IUserRepository _userRepo;
        private readonly IGenderRepository _genderRepository;
        private readonly ICountryRepository _countryRepository;

        public AuthController(UserManager<User> um, SignInManager<User> sm, AppDbContext db, ITokenService jwt, IValidationService valid, IUserRepository ur, IGenderRepository gr, ICountryRepository cr)
        {
            _um = um;
            _sm = sm;
            _db = db;
            _jwt = jwt;
            _valid = valid;
            _userRepo = ur;
            _genderRepository = gr;
            _countryRepository = cr;
        }

        [HttpPost]
        [Route("/register")]
        public async Task<ActionResult<User>> Register([FromBody]RegisterUser body)
        {
            ValidationResult emailValid = _verifyIfEmailExists(body.Email);
            ValidationResult pswdValid = _isPasswordValid(body.Password, body.ConfirmPassword);
            ValidationResult birthdateValid = _validateBirthDate(body.BirthDate);

            Gender? gender = _getGender(body.GenderId);
            Country? country = _getCountry(body.CountryId);
            
            if (!ModelState.IsValid || body == null)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            if (emailValid.IsValid && pswdValid.IsValid && body.TermsAccepted && gender != null && country != null && birthdateValid.IsValid)
            {
                User user = new User
                {
                    Email = body.Email,
                    UserName = $"{body.FName}{body.LName}",
                    Gender = gender, // todo
                    Country = country, // todo
                    AcceptedTerms = body.TermsAccepted,
                    UserActive = true,
                    BirthDate = body.BirthDate,
                    FName = body.FName,
                    LName = body.LName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                try
                {
                    user.SetUID(_db);

                    var res = await _um.CreateAsync(user, body.Password);

                    if (res.Succeeded)
                    {
                        User createdUser = _db.Users.First(u => u.UUID == user.UUID);
                        UserDTO toReturn = new UserDTO(createdUser);
                        return Ok(toReturn);
                    }
                    else
                    {
                        string errors = "";
                        foreach (var err in res.Errors)
                        {
                            errors += $"{err.Description}--";
                        }
                        return StatusCode(500, errors);
                    }
                }catch(Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            else
            {
                List<string> ex = [];
                foreach(var e in emailValid.Messages) { ex.Add(e); }
                foreach (var e in pswdValid.Messages) { ex.Add(e); }

                if (!body.TermsAccepted) { ex.Add("Terms must be accepted."); }
                if (gender == null) { ex.Add("Invalid gender."); }
                if (country == null) { ex.Add("Invalid country."); }
                if(!birthdateValid.IsValid) { ex.Add(birthdateValid.Messages[0]); }

                return BadRequest(new { ex });
            }
        }



        private ValidationResult _verifyIfEmailExists(string email)
        {
            var result = new ValidationResult { IsValid = true, Messages = [] };
            var findUserByEmail = _db.Users.Any(u => u.Email == email);
            if (findUserByEmail)
            {
                result.Invalidate("This email is already registered.");
            }
            return result;
        }
        
        private ValidationResult _isPasswordValid(string password, string confirmPswd) {
            ValidationResult result = new ValidationResult
            {
                IsValid = true,
                Messages = []
            };

            if(password == null) {
                result.Invalidate("Password cannot be null");
            }
            else
            {
                if (password.Length < 8 ) {
                    result.Invalidate("Password should have 8+ characters.");
                }

                if (!password.Any(char.IsUpper)) {
                    result.Invalidate("Password must contain at least 1 uppercase letter.");
                }
                if (!password.Any(char.IsLower))
                {
                    result.Invalidate("Password must contain at least 1 lowercase letter.");
                }
                if (!password.Any(char.IsNumber))
                {
                    result.Invalidate("Password must contain at least 1 digit.");
                }
                if(confirmPswd == null) { 
                    result.Invalidate("The field confirm password must be passed.");
                }

        [Authorize]
        [HttpGet]
        [Route("/users/all")]
        public async Task<ActionResult<List<UserDTO>>> GetAllUsers([FromQuery] int? count, [FromQuery] int? page)
                {
            if(page == null) { page = 1; }
            if(count == null) { count = 1; }

            List<UserDTO> users = await _userRepo.GetAllUsers((int)page, (int)count);

            return Ok(new { users });
        }
        
        [Authorize]
        [HttpPost]
        [Route("/users/delete")]
        public async Task<ActionResult> DeleteUser([FromBody] DeleteUser body) {
            int? uuid = body.uuid;
            string? password = body.password;
            try
        }
        private Country? _getCountry(int countryId)
        {
            if (_db.Country.Any(c => c.Id == countryId))
            {
                if(uuid == null) { throw new Exception("UUID is a mandatory parameter."); }
                
                User? user = _db.Users.FirstOrDefault(u => u.UUID == uuid) ?? throw new Exception("This user does not exist.");

                ClaimsIdentity claimsIdentity = User.Identity as ClaimsIdentity;
                var claims = claimsIdentity?.Claims;

                if(claims == null) { throw new Exception("No claims found in jwt."); }
        
                UserClaimsDTO userClaimsDTO = new UserClaimsDTO
        {
            ValidationResult result = new ValidationResult
            {
                    Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    Id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                    UUID = claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber)?.Value
            };

                User? requester = _db.Users.FirstOrDefault(u => (u.Id == userClaimsDTO.Id) && 
                                                                (u.UUID.ToString() == userClaimsDTO.UUID.ToString()) && 
                                                                (u.Email == userClaimsDTO.Email));

                if(requester == null) { throw new Exception("This user does not exist."); }

                if(user != requester) { throw new Exception("You don't have permission to delete this user."); }


                bool isPasswordCorrect = await _um.CheckPasswordAsync(user, password);
                if(!isPasswordCorrect) { throw new Exception("Password is incorrect."); }


                await _userRepo.DeleteUser((int)uuid);
                return NoContent();
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        private string _getErrorMessagesString(ValidationResult validationResult)
            {
            string errors = "";
            foreach(string msg in validationResult.Messages)
                int age = diff.Days / 365;
                if(age < 14)
                {
                errors += $"{msg}--";
                }
            }
            return errors;
            return result;
        }
    }
}
