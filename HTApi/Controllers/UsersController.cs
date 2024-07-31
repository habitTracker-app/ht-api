using HTApi.Data.Repos;
using HTAPI.Data;
using HTAPI.Models;
using HTApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HTApi.Models.ActionModels;
using HTApi.Models;
using HTAPI.Models.DemographicData;
using HTApi.DTOs;

namespace HTApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly UserManager<User> _um;
        private readonly SignInManager<User> _sm;
        private readonly AppDbContext _db;
        private readonly ITokenService _jwt;
        private readonly IValidationService _valid;
        private readonly IUserRepository _userRepo;
        private readonly IGenderRepository _genderRepository;
        private readonly ICountryRepository _countryRepository;

        public UsersController(UserManager<User> um, SignInManager<User> sm, AppDbContext db, ITokenService jwt, IValidationService valid, IUserRepository ur, IGenderRepository gr, ICountryRepository cr)
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

        [Authorize]
        [HttpPost]
        [Route("/user/updateUser")]
        public async Task<IActionResult> UpdateUserInformation(UpdateUserInfo body)
        {
            if (body == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ValidationResult birthdateValid = _valid.ValidateBirthDate(body.BirthDate);
            ValidationResult emailValid = _valid.ValidateIfCanRegisterEmail(body.Email);
            Gender? gender = _genderRepository.GetGender(body.GenderId);
            Country? country = _countryRepository.GetCountry(body.CountryId);
            
            if(gender == null)
            {
                return BadRequest("Invalid gender id.");
            }
            if (country == null)
            {
                return BadRequest("Invalid country id.");
            }
            if(!birthdateValid.IsValid)
            {
                return BadRequest(new { messages = birthdateValid.Messages });
            }
            if(!emailValid.IsValid)
            {
                return BadRequest(new { messages = emailValid.Messages });
            }

            try
            {
                User user = _jwt.GetUserByJWT();

                _userRepo.UpdateUser(user, body, gender, country);

                UserDTO userDto = new UserDTO(user);

                return Ok(userDto);

            }catch (Exception ex)
            {
                if (ex.Message.Contains("400"))
                {
                    return BadRequest($"{ex.Message}");
                }
                else
                {
                    return StatusCode(500, ex.Message);
                }
            }


        }
    }
}
