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
        [Required] public int UpdatedScore {  get; set; }
        [Required] public float ProgressMeasure { get; set; }

        [Required] public bool GoalComplete { get; set; }
    }
}
