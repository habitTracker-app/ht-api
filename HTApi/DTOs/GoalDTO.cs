using HTAPI.Models.ChallengeGoals;

namespace HTApi.DTOs
{
    public class GoalDTO
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string MeasureUnit { get; set; }
        public string FrequencyType { get; set; }

        public GoalDTO(Goal g)
        {
            this.Id = g.Id; 
            this.Action = g.Action; 
            this.MeasureUnit = g.MeasureUnit; 
            this.FrequencyType = g.Frequency.Type;
        }
    }
}
