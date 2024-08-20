using HTApi.DTOs;
using HTApi.Models.ActionModels;
using HTApi.Models.Exceptions;
using HTApi.Services;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.ChallengeGoals;
using HTAPI.Models.Challenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.OpenApi.Any;

namespace HTApi.Data.Repos
{
    public interface IChallengeRepository
    {
        Task<ChallengeDTO> GetChallenge(User user, string challengeId);

        Task<List<ChallengeDTO>> GetAllChallenges(User user);

        Task<ChallengeDTO> CreateChallenge(User user, CreateChallenge body);

        Task<ChallengeDTO> UpdateChallenge(User user, UpdateChallenge body);

        Task DeleteChallenge(User user, string challengeId);

        Task<ChallengeDTO> AddFriendToChallenge(AddFriendToChallenge body);
        Task<ChallengeDTO> RemoveFriendFromChallenge(RemoveFriendFromChallenge body);
        Task<List<ChallengeParticipantDTO>> GetAllChallengeParticipants(User user, string challengeId, int? page, int? pageSize);
        
    }
    public class ChallengeRepository : IChallengeRepository
    {
        public static AppDbContext? _db;
        public static IGoalRepository? _goal;
        public static IUtilitiesService? _utils;

        public ChallengeRepository(IServiceProvider sp)
        {
            _db = sp.GetRequiredService<AppDbContext>();
            _goal = sp.GetRequiredService<IGoalRepository>();
            _utils = sp.GetRequiredService<IUtilitiesService>();
        }

        public async Task<ChallengeDTO> GetChallenge(User user, string challengeId) {

            Challenge? challenge = await _db.Challenge
                                        .Where(c => c.Id.ToString() == challengeId)
                                        .Include(c => c.Owner)
                                        .Include(c => c.Participants)
                                        .Include(c => c.Category)
                                        .FirstOrDefaultAsync();
            if(challenge == null)
            {
                throw new BadRequestException("This challenge does not exist.", 404);
            }

            if(challenge.Owner != user && !challenge.Participants.Any(u => u == user))
            {
                throw new BadRequestException("Challenge not found", 404);
            }

            return new ChallengeDTO(challenge, _utils, _db, user);
        
        }

        public async Task<List<ChallengeDTO>> GetAllChallenges(User user)
        {
            List<Challenge> challenges = await _db.Challenge
                                            .Where(c => c.Owner.Id == user.Id || c.Participants.Contains(user))
                                            .Include(c => c.Owner)
                                            .Include(c => c.Participants)
                                            .Include(c => c.Category)
                                            .ToListAsync();
            List<ChallengeDTO> challengeDTOs = [];
            foreach(var challenge in challenges)
            {
                challengeDTOs.Add(new ChallengeDTO(challenge, _utils, _db, user));
            }
            return challengeDTOs;
        }

        public async Task<ChallengeDTO> CreateChallenge(User user, CreateChallenge body) { 
            ChallengeCategory cat = await _getChallengeCategoryById(body.ChallengeCategoryId);
            Frequency freq = _db.Frequency.FirstOrDefault(f => f.Id == body.GoalFrequencyId) ?? throw new BadRequestException("Invalid frequency id", 404);
            if (body.StartDate >= body.EndDate) { throw new BadRequestException("End date cannot be greater than start date", 406); }

            Challenge challenge = new()
            {
                Active = true,
                Category = cat,
                Description = body.ChallengeDescription,
                EndDate = body.EndDate ?? DateTime.MaxValue,
                StartDate = body.StartDate,
                Owner = user,
                Title = body.ChallengeTitle,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Participants = [user]
            };


            try
            {
                var result = _db.Challenge.Add(challenge);
                var res = await _db.SaveChangesAsync();
                var goal = await _goal.CreateGoal(body, result.Entity, freq);
                return new ChallengeDTO(result.Entity, _utils, _db, user);

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ChallengeDTO> UpdateChallenge(User user, UpdateChallenge body)
        {
            Challenge challenge = await _getChallengeById(body.ChallengeId);

            if(challenge.Owner != user) { throw new BadRequestException("Only owners can edit a challenge.", 401); }

            ChallengeCategory? cat = await _getChallengeCategoryById(body.ChallengeCategoryId);

            challenge.StartDate = body.StartDate;
            challenge.EndDate = body.EndDate ?? DateTime.MaxValue;
            challenge.Description = body.ChallengeDescription;
            challenge.Title = body.ChallengeTitle;
            challenge.Category = cat;
            challenge.Active = body.IsActive;
            challenge.UpdatedAt = DateTime.UtcNow;


            try
            {
                var result = _db.Challenge.Update(challenge);
                await _db.SaveChangesAsync();
                return new ChallengeDTO(result.Entity, _utils, _db, user);
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteChallenge(User user, string challengeId)
        {

            Challenge challenge = await _getChallengeById(challengeId);

            if(user != challenge.Owner) { throw new BadRequestException("Only the owner can delete a challenge.", 403); }

            try
            {
                _db.Challenge.Remove(challenge);
                await _db.SaveChangesAsync();
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ChallengeDTO> AddFriendToChallenge(AddFriendToChallenge body)
        {
            Challenge challenge = await _getChallengeById(body.ChallengeId);

            User requester = _db.Users.FirstOrDefault(u => u.UUID == body.UserUuid) ?? throw new BadRequestException("The user trying to add a friend does not exist", 404);

            if(challenge.Owner != requester) { throw new BadRequestException("Only owners can add friends to a challenge", 401); }

            User userToAdd = _db.Users.FirstOrDefault(u => u.UUID == body.TargetUuid) ?? throw new BadRequestException("The user you want to add does not exist.", 404);

            if (challenge.Participants.Any(u => u == userToAdd)) { throw new BadRequestException("The user you're trying to add is already a challenge participant", 403); }

            bool areUsersFriends = _db.Friendship.Any(f => f.Target.Id == userToAdd.Id && f.Requester.Id == requester.Id);

            if (!areUsersFriends) { throw new BadRequestException("You cannot add this user because you are not friends.", 403); }

            challenge.Participants.Add(userToAdd);
            challenge.UpdatedAt = DateTime.UtcNow;


            try
            {
                var result = _db.Challenge.Update(challenge);
                await _db.SaveChangesAsync();

                return new ChallengeDTO(result.Entity, _utils, _db, requester);
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<ChallengeDTO> RemoveFriendFromChallenge(RemoveFriendFromChallenge body)
        {
            Challenge challenge = await _getChallengeById(body.ChallengeId);

            User requester = _db.Users.FirstOrDefault(u => u.UUID == body.UserUuid) ?? throw new BadRequestException("The user trying to remove a friend does not exist", 404);

            if (challenge.Owner != requester) { throw new BadRequestException("Only owners can remove friends from a challenge", 401); }

            User targetUser = _db.Users.FirstOrDefault(u => u.UUID == body.TargetUuid) ?? throw new BadRequestException("The user you want to remove does not exist.", 404);

            if (!challenge.Participants.Any(u => u == targetUser)) { throw new BadRequestException("The user you're trying to remove is not a challenge participant.", 403); }

            bool areUsersFriends = _db.Friendship.Any(f => f.Target.Id == targetUser.Id && f.Requester.Id == requester.Id);

            if (!areUsersFriends) { throw new BadRequestException("You cannot remove this user because you are not friends.", 403); }

            challenge.Participants.Remove(targetUser);
            challenge.UpdatedAt = DateTime.UtcNow;


            try
            {
                var result = _db.Challenge.Update(challenge);
                await _db.SaveChangesAsync();

                return new ChallengeDTO(result.Entity, _utils, _db, requester);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<ChallengeParticipantDTO>> GetAllChallengeParticipants(User user, string challengeId, int? page, int? pageSize)
        {
            Challenge challenge = await _getChallengeById(challengeId);
            if (challenge.Owner != user && !challenge.Participants.Any(u => u == user)) throw new BadRequestException("You cannot view this challenge's participants.", 403);

            pageSize = _validatePageSize(pageSize);
            page = _validatePageNumber(page, (int)pageSize, challenge.Participants.Count);
            try
            {
                List<User> paginated = await _db.Users
                                            .Skip(((int)(page) - 1) * (int)pageSize)
                                            .Take((int)pageSize)
                                            .Include(u => u.Challenges)
                                            .ToListAsync();


                Goal challengeGoal = _db.Goal
                                        .Where(g => g.Challenge == challenge)
                                        .Include(g => g.Frequency)
                                        .OrderBy(g => g)
                                        .Last()
                                        ?? throw new BadRequestException("No goals found for this challenge", 404);
                
                List<ChallengeParticipantDTO> response = [];

                foreach(User u in paginated)
                {
                    if (!u.Challenges.Any(c => c == challenge)) continue;

                    DateTime now = DateTime.Now;
                    var (start, end) = _utils.GetPeriodStartEnd(challengeGoal.Frequency.Type, now);
                    Progress? latestProgress = _db.Progress.Where(p => p.User.Id == u.Id && p.Date >= start && p.Date <= end).OrderBy(p => p.Date).LastOrDefault();

                    ChallengeParticipantDTO participantDTO = new()
                    {
                        Name = u.FName + " " + u.LName,
                        PUID = u.UUID,
                        ProgressPercentage = latestProgress?.UpdatedProgressPercentage ?? 0,
                        Score = latestProgress?.UpdatedScore ?? 0
                    };
                    response.Add(participantDTO);
                }

                return response;
            }catch(Exception ex)
            {
                throw;
            }
        }
        private int _validatePageNumber(int? page, int pageSize, int itemCount)
        {
            const int MIN_PAGE_VALUE = 1;
            if (page == null) { return MIN_PAGE_VALUE; }
            else
            {
                if (page < 1) { throw new BadRequestException("Invalid page.", 406); }
                float pages = (itemCount / pageSize) + 1;

                if (Math.Round(pages) < page) { throw new BadRequestException("This page does not exist", 404); }

                return (int)page;
            }
        }
        private int _validatePageSize(int? pageSize)
        {
            const int MAX_PAGE_SIZE = 20;
            if (pageSize == null) { return MAX_PAGE_SIZE; }
            else
            {
                if (pageSize > 20) { throw new BadRequestException("Max items per page is 20", 406); }
                if (pageSize < 1) { throw new BadRequestException("Min items per page is 1", 406); }
                return (int)pageSize;
            }
        }
        private async Task<Challenge> _getChallengeById(string challengeId)
        {
            Challenge? challenge = await _db.Challenge
                                        .Where(c => c.Id.ToString() == challengeId)
                                        .Include(c => c.Owner)
                                        .Include(c => c.Participants)
                                        .Include(c => c.Category)
                                        .FirstOrDefaultAsync();


            if (challenge == null) { throw new BadRequestException("This challenge does not exist.", 404); }

            return challenge;
        }

        private async Task<ChallengeCategory> _getChallengeCategoryById(string categoryId)
        {
            ChallengeCategory? cat = await _db.ChallengeCategory.Where(c => c.Id.ToString() == categoryId).FirstOrDefaultAsync();

            if (cat == null) { throw new BadRequestException("Invalid challenge category", 404); }

            return cat;
        }
        
        private async Task<Frequency> _getFrequencyById(int freqId)
        {
            return await _db.Frequency.FirstOrDefaultAsync(f => f.Id == freqId) ?? throw new BadRequestException("Invalid frequency id", 404);
        }
    }
}
