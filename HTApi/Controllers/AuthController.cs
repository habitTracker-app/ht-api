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
                if(confirmPswd != password)
                {
                    result.Invalidate("Passwords don't match.");
                }
            }

            return result;
        }
        
        private Gender? _getGender(int genderId)
        {
            if(_db.Gender.Any(g => g.Id == genderId))
            {
                return _db.Gender.Find(genderId);
            }
            return null;
        }
        private Country? _getCountry(int countryId)
        {
            if (_db.Country.Any(c => c.Id == countryId))
            {
                return _db.Country.Find(countryId);
            }
            return null;
        }
        
        private ValidationResult _validateBirthDate(DateTime bd)
        {
            ValidationResult result = new ValidationResult
            {
                IsValid = true,
                Messages = []
            };

            if(bd.Date >= DateTime.UtcNow.Date)
            {
                result.Invalidate("Birthdate must be before today.");
            }else
            {
                TimeSpan diff = DateTime.UtcNow - bd;

                int age = diff.Days / 365;
                if(age < 14)
                {
                    result.Invalidate("User must be at least 14 years old to register.");
                }
            }

            return result;
        }
    }
}
