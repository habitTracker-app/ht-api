using HTApi.Data.Repos;
using HTApi.DTOs;
using HTApi.Models.ActionModels;
using HTApi.Models.Exceptions;
using HTApi.Services;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.Challenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengesController : ControllerBase
    {
        public static AppDbContext? _db;
        public static IChallengeRepository? _cr;
        public static IProgressRepository? _pr;
        public static ITokenService? _jwt;

        public ChallengesController(AppDbContext db, IChallengeRepository cr, IProgressRepository pr, ITokenService jwt)
        {
            _db = db;
            _cr = cr;
            _pr = pr;
            _jwt = jwt;
        }

        [Authorize]
        [HttpGet]
        [Route("/challenges/get")]
        public async Task<IActionResult> GetChallengeById([FromQuery] string challengeId)
        {
            try
            {
                User user = _jwt.GetUserByJWT();

                ChallengeDTO challenge = await _cr.GetChallenge(user, challengeId);

                return Ok(challenge);

            }catch(BadRequestException bex)
            {
                return BadRequest(bex.Message);
            }catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("/challenges/all")]
        public async Task<IActionResult> GetUserChallenges()
        {
            try
            {
                User user = _jwt.GetUserByJWT();

                List<ChallengeDTO> challenges = await _cr.GetAllChallenges(user);

                return Ok(challenges);
            }catch(BadRequestException e)
            {
                return BadRequest(e.Message);
            }catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("/challenges/create")]
        public async Task<IActionResult> CreateChallenge(CreateChallenge body)
        {
            if (body == null || !ModelState.IsValid) { return BadRequest(ModelState); }

            try
            {
                User user = _jwt.GetUserByJWT();
                ChallengeDTO challenge = await _cr.CreateChallenge(user, body);

                return StatusCode(201, new { challenge });
            }catch(BadRequestException bex)
            {
                return BadRequest(bex.Message);
            }catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("/challenges/getParticipants")]
        public async Task<IActionResult> GetChallengeParticipants([FromQuery] string challengeId, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            User user = _jwt.GetUserByJWT();
            try
            {
                List<ChallengeParticipantDTO> list = await _cr.GetAllChallengeParticipants(user, challengeId, page, pageSize);
                return StatusCode(200, list);
            }catch(BadRequestException bex)
            {
                return BadRequest(bex.Message);
            }catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/challenges/edit")]
        public async Task<IActionResult> EditChallenge(UpdateChallenge body)
        {
            if(body == null || !ModelState.IsValid) { return BadRequest(ModelState); }

            try
            {
                User user = _jwt.GetUserByJWT();
                ChallengeDTO challenge = await _cr.UpdateChallenge(user, body);
                return Ok(challenge);
            }catch(BadRequestException bex)
            {
                return BadRequest(bex.Message);
            }catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("/challenges/delete")]
        public async Task<IActionResult> DeleteChallenge([FromQuery] string challengeId)
        {
            try
            {
                User user = _jwt.GetUserByJWT();
                await _cr.DeleteChallenge(user, challengeId);

                return NoContent();
            }catch(BadRequestException bex)
            {
                return BadRequest(bex.Message);
            }catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/challenges/addFriend")]
        public async Task<IActionResult> AddFriendToChallenge(AddFriendToChallenge body)
        {
            try
            {
                ChallengeDTO c = await _cr.AddFriendToChallenge(body);
                return Ok(c);
            }catch(BadRequestException e)
            {
                return BadRequest(e.Message);
            }catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("/challenges/removeFriend")]
        public async Task<IActionResult> RemoveFriendFromChallenge(RemoveFriendFromChallenge body)
        {
            try
            {
                ChallengeDTO c = await _cr.RemoveFriendFromChallenge(body);
                return Ok(c);
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

        [Authorize]
        [HttpPost]
        [Route("/challenges/progress")]
        public async Task<IActionResult> UpdateProgress(RegisterProgress body)
        {
            if(body == null || !ModelState.IsValid) { return BadRequest(ModelState); }

            try
            {
                User user = _jwt.GetUserByJWT();
                ChallengeDTO p = await _pr.CreateProgress(user,body);
                return StatusCode(201, p);
            }catch(BadRequestException bex)
            {
                return BadRequest(bex.Message);
            }catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("/challenges/progress")]
        public async Task<IActionResult> UpdateProgress([FromQuery] string progressId)
        {
            try
            {
                User user = _jwt.GetUserByJWT();
                await _pr.DeleteProgress(user, progressId);
                return NoContent();
            }
            catch (BadRequestException bex)
            {
                return BadRequest(bex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
