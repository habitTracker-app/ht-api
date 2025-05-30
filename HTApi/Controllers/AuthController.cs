﻿using HTApi.Data.Repos;
using HTApi.DTOs;
using HTApi.Models;
using HTApi.Models.ActionModels;
using HTApi.Models.Exceptions;
using HTApi.Services;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.ActionModels;
using HTAPI.Models.DemographicData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            ValidationResult emailValid = _valid.ValidateIfCanRegisterEmail(body.Email);
            ValidationResult pswdValid = _valid.ValidatePassword(body.Password, body.ConfirmPassword);
            ValidationResult birthdateValid = _valid.ValidateBirthDate(body.BirthDate);

            Gender? gender = _genderRepository.GetGender(body.GenderId);
            Country? country = _countryRepository.GetCountry(body.CountryId);
            
            if (!ModelState.IsValid || body == null)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { ModelState });
            }

            if (emailValid.IsValid && pswdValid.IsValid && body.TermsAccepted && gender != null && country != null && birthdateValid.IsValid)
            {
                try { 
                    UserDTO userDto = await _userRepo.CreateUser(body, gender, country);
                    return Ok(userDto);
                } catch (Exception ex) {
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


        [HttpPost]
        [Route("/signin")]
        public async Task<ActionResult<User>> Login([FromBody] LoginUser body)
        {
            if (body == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            ValidationResult emailExists = _valid.ValidateIfCanSigninEmail(body.Email);

            if (!emailExists.IsValid) { return StatusCode(400, new { emailExists.Messages }); }

            try
            {
                User user = _db.Users.First(u => u.Email == body.Email);
                var attempt = await _sm.PasswordSignInAsync(user, body.Password, body.RememberMe, false);

                if (attempt.Succeeded)
                {
                    if(!user.UserActive) { await _userRepo.UpdateUserActiveStatus(user); }
                 
                    var token = _jwt.GenerateJwtToken(user.Email, body.RememberMe, user.UUID, user.Id);
                    UserDTO userDto = new UserDTO(_db.Users.First(u => u.Id == user.Id));

                    return Ok(new { Token = token, User = userDto });
                }
                else
                {
                    if (attempt.IsNotAllowed) { return Unauthorized("Invalid credentials - password"); }

                    throw new BadRequestException("Password incorrect.", 401);
                }
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
       
        private string _getErrorMessagesString(ValidationResult validationResult)
        {
            string errors = "";
            foreach(string msg in validationResult.Messages)
            {
                errors += $"{msg};";
            }
            return errors;
        }
    }
}
