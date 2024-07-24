using HTAPI.Data;
using HTAPI.Models.ChallengeGoals;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTAPI.Models.Challenges
{
    public class Progress
    {
        public Guid Id { get; set; }

        [Required] [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("Goal")]
        public Guid GoalId { get; set; }
        public virtual Goal Goal { get; set; }

        [Required] public int UpdatedProgressPercentage { get; set; }
        [Required] public float ProgressMeasure { get; set; }

        [Required] public bool GoalComplete { get; set; }



        public void SetProgressPercentage(AppDbContext db)
        {
            Goal? goal = db.Goal.ToList().Find(x => x.Id == this.GoalId);

            ICollection<Progress> progresses = db.Progress.ToList().FindAll(p => p.GoalId == this.GoalId);

            Challenge challenge = db.Challenge.ToList().Find(c => c.Id == goal.ChallengeId);

            if (goal != null)
            {
                float totalProgress = this.ProgressMeasure;

                if (goal.Frequency.Type == "daily") {
                    ICollection<Progress> todaysProgresses = progresses.ToList().FindAll(p => (p.Date == this.Date));

                    foreach (var item in todaysProgresses)
                    {
                        totalProgress += item.ProgressMeasure;
                    }
                }
                else if(goal.Frequency.Type == "weekly")
                {
                    DateTime registerDate = this.Date;
                    DateTime weekEnd = GetWeekEnd(registerDate);
                    DateTime weekStart = GetWeekStart(registerDate);
                    totalProgress = GetTotalProgress(weekStart, weekEnd, progresses);
                }
                else if(goal.Frequency.Type == "monthly")
                {
                    DateTime registerDate = this.Date;
                    DateTime monthEnd = GetMonthEnd(registerDate);
                    DateTime monthStart = GetMonthStart(registerDate);
                    totalProgress = GetTotalProgress(monthStart, monthEnd, progresses);
                }
                else if (goal.Frequency.Type == "annually")
                {
                    DateTime registerDate = this.Date;
                    DateTime monthEnd = GetMonthEnd(registerDate);
                    DateTime monthStart = GetMonthStart(registerDate);
                    ICollection<Progress> thisYearsProgresses = progresses.ToList().FindAll(p => (p.Date.Year == this.Date.Year));
                    foreach (var item in thisYearsProgresses)
                    {
                        totalProgress += item.ProgressMeasure;
                    }
                }
                else if(goal.Frequency.Type == "biweekly") {
                    DateTime registerDate = this.Date;
                    DateTime fortnightEnd = GetFortnightEnd(registerDate);
                    DateTime fortnightStart = GetFortnightStart(registerDate);
                    totalProgress = GetTotalProgress(fortnightStart, fortnightEnd, progresses);
                }
                else if(goal.Frequency.Type == "biannual") {
                    DateTime registerDate = this.Date;
                    DateTime semesterEnd = GetSemesterEnd(registerDate);
                    DateTime semesterStart = GetSemesterStart(registerDate);
                    totalProgress = GetTotalProgress(semesterStart, semesterEnd, progresses);
                }

                float progressPercent = (totalProgress * 100) / goal.Amount;
                this.UpdatedProgressPercentage = (int)progressPercent;
                this.SetGoalStatus();
            }
        }
        
        
        
        
        private void SetGoalStatus()
        {
            if (this.UpdatedProgressPercentage >= 100)
            {
                this.GoalComplete = true;
            }
            else
            {
                this.GoalComplete = false;
            }
        }
        private float GetTotalProgress(DateTime startDate, DateTime endDate, ICollection<Progress> progresses) {
            float totalProgress = 0;
            
            ICollection<Progress> currentProgresses = progresses.ToList().FindAll(p => (p.Date >= startDate) && (p.Date <= endDate));
            foreach (var item in currentProgresses)
            {
                totalProgress += item.ProgressMeasure;
            }

            return totalProgress;
        }
        
        private DateTime GetFortnightStart(DateTime date) {
            if(date.Day <= 15)
            {
                return new DateTime(date.Year, date.Month, 1);
            }
            else
            {
                return new DateTime(date.Year, date.Month, 16);
            }
        }
        private DateTime GetFortnightEnd(DateTime date) {

            if (date.Day <= 15)
            {
                return new DateTime(date.Year, date.Month, 15);
            }
            else
            {
                int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                return new DateTime(date.Year, date.Month, daysInMonth);
            }
        }

        private DateTime GetSemesterStart(DateTime date)
        {
            if(date.Month <= 6)
            {
                return new DateTime(date.Year, 1, 1);
            }
            else
            {
                return new DateTime(date.Year, 7, 1);
            }
        }

        private DateTime GetSemesterEnd(DateTime date)
        {
            if (date.Month <= 6)
            {
                return new DateTime(date.Year, 6, 30);
            }
            else
            {
                return new DateTime(date.Year, 12, 31);
            }
        }

        private DateTime GetMonthStart(DateTime date) {
            return new DateTime(date.Year, date.Month, 1);
        }
        private DateTime GetMonthEnd(DateTime date)
        {
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            return new DateTime(date.Year, date.Month, daysInMonth);
        }
        private DateTime GetWeekStart(DateTime date)
        {
            DayOfWeek startDay = DayOfWeek.Sunday;
            
            int diff = (7 + (date.DayOfWeek - startDay)) % 7;

            return date.AddDays(-1 * diff).Date;
        }

        private DateTime GetWeekEnd(DateTime date) { 
            DayOfWeek endDay = DayOfWeek.Saturday;
            int diff = (7 + (date.DayOfWeek - endDay)) % 7;

            return date.AddDays(diff).Date;
        }
    }
}
