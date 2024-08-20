using HTApi.Models.ActionModels;
using HTAPI.Data;
using HTAPI.Models.ChallengeGoals;
using HTAPI.Models.Challenges;
using Microsoft.EntityFrameworkCore;

namespace HTApi.Data.Repos
{
    public interface IGoalRepository
    {
        Task<Goal> CreateGoal (CreateChallenge body, Challenge challenge, Frequency freq);
        Task<Goal?> GetGoalById(string id);
    }
    public class GoalRepository : IGoalRepository 
    {
        private static AppDbContext? _db;
        public GoalRepository(IServiceProvider sp)
        {
            _db = sp.GetRequiredService<AppDbContext>();
        }

        public async Task<Goal> CreateGoal(CreateChallenge body, Challenge challenge, Frequency freq)
        {
            Goal goal = new()
            {
                Action = body.GoalAction,
                Amount = body.GoalAmount,
                MeasureUnit = body.GoalMeasureUnit,
                Frequency = freq,
                Challenge = challenge
            };

            try
            {
                var result = await _db.Goal.AddAsync(goal);
                await _db.SaveChangesAsync();
                return result.Entity;
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Goal?> GetGoalById(string id)
        {
            Goal g = _db.Goal.Where(x => x.Id.ToString() == id).Include(x => x.Challenge).Include(x => x.Frequency).OrderBy(x => x.Id).Last();
            return g;
        }
    }
}
