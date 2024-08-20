using System.ComponentModel.DataAnnotations;

namespace HTApi.Models.ActionModels
{
    public class UpdateChallenge
    {
        public string ChallengeId { get; set; }
        public string ChallengeTitle { get; set; }
        public string ChallengeDescription { get; set; }
        public string ChallengeCategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } = DateTime.MaxValue;
        public bool IsActive { get; set; }



        // goal data
        [Required]
        public string GoalAction { get; set; }
        [Required]
        public float GoalAmount { get; set; }
        [Required]
        public string GoalMeasureUnit { get; set; }
        [Required]
        public int GoalFrequencyId { get; set; }
    }
}
