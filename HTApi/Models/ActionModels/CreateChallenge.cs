using System.ComponentModel.DataAnnotations;

namespace HTApi.Models.ActionModels
{
    public class CreateChallenge
    {
        [Required]
        public string ChallengeTitle { get; set; }
        
        public string ChallengeDescription { get; set;}
        
        [Required]
        public string ChallengeCategoryId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
     
        public DateTime? EndDate { get; set; } = DateTime.MaxValue;


        // goal data
        [Required]
        public string GoalAction {  get; set; }
        [Required]
        public float GoalAmount { get; set; }
        [Required]
        public string GoalMeasureUnit { get; set; }
        [Required]
        public int GoalFrequencyId { get; set; }


        public Boolean IsActive { get; set; } = true;
    }
}
