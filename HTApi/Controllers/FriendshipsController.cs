using HTApi.Data.Repos;
using HTApi.DTOs;
using HTApi.Models.ActionModels;
using HTApi.Models.Exceptions;
using HTApi.Services;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.Friendships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HTApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendshipsController : ControllerBase
    {
        private readonly UserManager<User> _um;
        private readonly SignInManager<User> _sm;
        private readonly AppDbContext _db;
        private readonly ITokenService _jwt;
        private readonly IFriendshipRepository _fr;

        public FriendshipsController(UserManager<User> um, SignInManager<User> sm, AppDbContext db, ITokenService jwt, IFriendshipRepository fr)
        {
            _um = um;
            _sm = sm;
            _db = db;
            _jwt = jwt;
            _fr = fr;
        }

        [Authorize]
        [HttpPost]
        [Route("/friends/add")]
        public async Task<ActionResult> SendFriendRequest(AddFriend body)
        {
            if (body == null || !ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            try
            {
                User user = _jwt.GetUserByJWT();

                if (user.UUID.ToString() != body.RequesterUuid.ToString())
                {
                    throw new BadRequestException("You cannot perform actions for this user.", 401);
                }

                FriendshipDTO f = await _fr.CreateFriendship(body);

                return Created("", new { f });
                
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
        [Route("/friends/answer-request")]
        public async Task<ActionResult<FriendshipDTO>> AnswerFriendRequest(AnswerFriendRequest body)
        {
            if(body == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User user = _jwt.GetUserByJWT();

                FriendshipDTO f = await _fr.AnswerRequest(body, user);

                return Ok(f);

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
        [Route("/friends/cancel-request")]
        public async Task<ActionResult> CancelFriendRequest(CancelFriendRequest body)
        {
            if(body ==null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User user = _jwt.GetUserByJWT();
                FriendshipDTO f = await _fr.CancelRequest(body, user);

                return Ok(f);
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


        // TODO - handle authorization better
        [Authorize]
        [HttpGet]
        [Route("/friends/requests")]
        public ActionResult<List<FriendshipDTO>> GetAllFriendRequests()
        {
            User user = _jwt.GetUserByJWT();

            return Ok(_fr.GetAllFriendRequests(user));
        }
        

    }
}
