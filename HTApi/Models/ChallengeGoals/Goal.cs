using HTAPI.Models.Challenges;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTAPI.Models.ChallengeGoals
{
    public class Goal
    {
        public Guid Id { get; set; }

        [Required] public string Action { get; set; }
        [Required] public float Amount { get; set; }

        [Required] public string MeasureUnit { get; set; }

        [ForeignKey("Frequency")]
        public int FrequencyId { get; set; }
        public virtual Frequency Frequency { get; set; }


        [ForeignKey("Challenge")]
        public Guid ChallengeId { get; set; }
        public virtual Challenge Challenge { get; set; }

        public ICollection<Progress> progresses { get; set; }

    }
}
