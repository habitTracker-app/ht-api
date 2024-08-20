using HTApi.Services;
using HTAPI.Data;
using HTAPI.Models;
using HTAPI.Models.ChallengeGoals;
using HTAPI.Models.Challenges;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace HTApi.DTOs
{
    public class ChallengeDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public CategoryDTO Category { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public GoalDTO Goal { get; set; }
        // user related
        public int OwnerPuid { get; set; }
        public ChallengeParticipantDTO CurrentUser { get; set; }
        public Top3DTO Top3 { get; set; }

        public ChallengeDTO(Challenge c, IUtilitiesService utils, AppDbContext db, User user)
        {
            this._setGeneralAttributes(c);
            
            Goal goal = this._setGoal(c, db);
            var (start, end) = _getStartEndRange(utils, goal);

            this._setCurrentUser(user, start, end, c);

            this._setTop3(c, start, end);
        }
        private void _setGeneralAttributes(Challenge c)
        {
            this.Id = c.Id;
            this.Title = c.Title;
            this.Description = c.Description;
            this.Category = new CategoryDTO(c.Category);
            this.StartDate = c.StartDate;
            this.EndDate = c.EndDate;
            this.IsActive = c.Active;
            this.CreatedAt = c.CreatedAt;
            this.UpdatedAt = c.UpdatedAt;

            this.OwnerPuid = c.Owner.UUID;
        }
        private Goal _setGoal(Challenge c, AppDbContext db)
        {
            Goal goal = db.Goal.Where(g => g.Challenge.Id == c.Id)
                                        .Include(g => g.Frequency)
                                        .OrderBy(g => g.Id)
                                        .Last() ?? throw new Exception("No goal registered for this challenge");
            this.Goal = new GoalDTO(goal);

            return goal;
        }

        private Tuple<DateTime, DateTime> _getStartEndRange(IUtilitiesService utils, Goal goal)
        {
            DateTime today = DateTime.Now;
            string goalFrequency = goal.Frequency.Type ?? throw new Exception("No frequency registered for this challenge");
            return utils.GetPeriodStartEnd(goalFrequency, today);
        }

        private void _setCurrentUser(User user, DateTime start, DateTime end, Challenge c)
        {
            User currentUser = c.Participants.Where(p => p == user).OrderBy(p => p.CreatedAt).Last();
            Progress? currentUserProgress = currentUser.Progresses
                                            .Where(p => p.Goal.Challenge == c)?
                                            .Where(p => p.Date >= start && p.Date <= end)?
                                            .OrderBy(p => p.Date)?
                                            .LastOrDefault();

            ChallengeParticipantDTO currentUserDTO = new()
            {
                PUID = currentUser.UUID,
                Name = currentUser.FName + " " + currentUser.LName,
                ProgressPercentage = currentUserProgress == null ? 0 : currentUserProgress.UpdatedProgressPercentage,
                Score = currentUserProgress == null ? 0 : currentUserProgress.UpdatedScore
            };
            this.CurrentUser = currentUserDTO;
        }

        private void _setTop3(Challenge c, DateTime start, DateTime end)
        {
            Top3DTO top3 = new(c.Participants, start, end);

            this.Top3 = top3;
        }
    }
}


