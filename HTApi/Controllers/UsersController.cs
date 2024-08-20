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
using HTApi.Models.Exceptions;

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
        [HttpGet]
        [Route("/users/all")]
        public ActionResult<List<UserDTO>> GetAllUsers([FromQuery] int? count, [FromQuery] int? page)
        {
            if (page == null) { page = 1; }
            if (count == null) { count = 1; }

            List<UserDTO> users = _userRepo.GetAllUsers((int)page, (int)count);

            return Ok(new { users });
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

                await _userRepo.UpdateUser(user, body, gender, country);

                UserDTO userDto = new UserDTO(user);

                return Ok(userDto);

            }
            catch(BadRequestException bex)
            {
                return BadRequest(bex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }


        }

        [Authorize]
        [HttpPost]
        [Route("/user/inactivate")]
        public async Task<IActionResult> InactivateAccount(InactivateAccount body)
        {
            if(body == null || !ModelState.IsValid) { return BadRequest(ModelState); }

            try
            {
                User user = _jwt.GetUserByJWT();

                await _checkPassword(user, body.Password);
                await _userRepo.UpdateUserActiveStatus(user);
                await _jwt.BlockJWT();

                return Ok(user);
            }
            catch(BadRequestException bex)
            {
                return BadRequest(bex.Message);

            }
            catch (Exception ex) 
            {

                return StatusCode(500, ex.Message);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("/user/delete")]
        public async Task<ActionResult> DeleteUser([FromBody] DeleteUser body)
        {
            int? uuid = body.uuid;
            string? password = body.password;
            try
            {
                if (uuid == null) { throw new BadRequestException("UUID is a mandatory parameter.", 406); }

                User? user = _db.Users.FirstOrDefault(u => u.UUID == uuid) ?? throw new BadRequestException("This user does not exist.", 404);

                User? requester = _jwt.GetUserByJWT();

                if (user != requester) { throw new BadRequestException("You don't have permission to delete this user.", 401); }

                await _checkPassword(user, password);
                await _userRepo.DeleteUser((int)uuid);
                await _jwt.BlockJWT();
                return NoContent();
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/user/change-pswd")]
        public async Task<IActionResult> ChangePassword(ChangePassword body)
        {
            if(body == null || !ModelState.IsValid) { return BadRequest(ModelState); }

            try
            {
                User user = _jwt.GetUserByJWT();

                await _userRepo.ChangeUserPassword(body, user);

                return Ok();
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            

            
            
        }

        private async Task<bool> _checkPassword(User user, string password)
        {
            bool isPasswordCorrect = await _um.CheckPasswordAsync(user, password);
            if (!isPasswordCorrect) { throw new BadRequestException("Password is incorrect.", 403); }
            return isPasswordCorrect;
        }
    }
}
