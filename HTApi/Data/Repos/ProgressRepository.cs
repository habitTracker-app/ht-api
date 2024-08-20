using HTApi.DTOs;
using HTApi.Models.ActionModels;
using HTApi.Models.Exceptions;
using HTApi.Services;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.ChallengeGoals;
using HTAPI.Models.Challenges;
using Microsoft.EntityFrameworkCore;

namespace HTApi.Data.Repos
{
    public interface IProgressRepository
    {
        Task<ChallengeDTO> CreateProgress(User user, RegisterProgress body);
        Task DeleteProgress(User user, string id);

    }
    public class ProgressRepository : IProgressRepository
    {
        private static AppDbContext? _db;
        private static IGoalRepository? _goal;
        private static IChallengeRepository? _challenge;
        private static IUtilitiesService? _utils;

        private static readonly int _pointsPerProgress = 15;

        public ProgressRepository(IServiceProvider sp)
        {
            _db = sp.GetRequiredService<AppDbContext>();
            _goal = sp.GetRequiredService<IGoalRepository>();
            _challenge = sp.GetRequiredService<IChallengeRepository>();
            _utils = sp.GetRequiredService<IUtilitiesService>();
        }

        public async Task<ChallengeDTO> CreateProgress(User user, RegisterProgress body)
        {
            Goal goal = await _goal.GetGoalById(body.GoalId) ?? throw new BadRequestException("This goal does not exist", 404);
            ChallengeDTO challenge = await _challenge.GetChallenge(user, goal.Challenge.Id.ToString());

            try
            {
                DateTime registerTime = DateTime.UtcNow;
                var (totalProgress, totalProgressPercentage, isComplete, score) = await _calculateProgress(goal, user, registerTime, body.ProgressAmount);
                
                Progress p = new()
                {
                    Date = DateTime.Now,
                    Goal = goal,
                    User = user,
                    GoalComplete = isComplete,
                    ProgressMeasure = body.ProgressAmount,
                    UpdatedProgressPercentage = totalProgressPercentage,
                    UpdatedScore = score,
                };

                var instance = await _db.Progress.AddAsync(p);

                await _db.SaveChangesAsync();

                Challenge c = await _db.Challenge.FirstOrDefaultAsync(c => c.Id == challenge.Id) ?? throw new BadRequestException("This challenge no longer exists", 404);
                return new ChallengeDTO(c, _utils, _db, user);

            } catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteProgress(User user, string id)
        {
            Progress p = _db.Progress.Where(p => p.Id.ToString() == id).Include(p => p.Goal).OrderBy(p => p.Date).Last() ?? throw new BadRequestException("This progress does not exist.", 404);
            if (p.User != user) throw new BadRequestException("You cannot delete this progress because you did not create it.", 403);
            Goal goal = _db.Goal.Where(g => g.Id == p.Goal.Id).Include(g => g.Frequency).OrderBy(g => g.Id).Last() ?? throw new BadRequestException("This progress is not related to a goal.", 406);
            var (start, end) = _utils.GetPeriodStartEnd(goal.Frequency.Type, p.Date);
            DateTime now = DateTime.UtcNow;
            if (now > end || now < start) throw new BadRequestException("You can no longer delete this progress.", 403);

            try
            {
                var result = _db.Progress.Remove(p);
                await _db.SaveChangesAsync();
            }catch(Exception ex)
            {
                throw;
            }
        }

        private async Task<Tuple<float, int, bool, int>> _calculateProgress(Goal goal, User user, DateTime registerDate, float amount)
        {
            var (start, end) = _utils.GetPeriodStartEnd(goal.Frequency.Type, registerDate);
            List<Progress> progressesList = await _getUserProgressesByDate(user, goal, start, end);

            float totalProgress = progressesList.Sum(p => p.ProgressMeasure) + amount;

            float totalProgressPercentage = (totalProgress * 100) / goal.Amount;
            
            bool isComplete = totalProgressPercentage >= 100;

            int score = (int)(totalProgressPercentage * _pointsPerProgress);
            return new( totalProgress, (int)totalProgressPercentage, isComplete, score );
        }

        private async Task<List<Progress>> _getUserProgressesByDate(User user, Goal goal, DateTime start, DateTime end)
        {
            return await _db.Progress.Where(p => p.User == user)
                                        .Where(p => p.Goal == goal)
                                        .Where(p => p.Date >= start && p.Date <= end)
                                        .ToListAsync();
        }
    }
}
