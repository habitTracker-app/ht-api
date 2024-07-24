using System.ComponentModel.DataAnnotations;

namespace HTAPI.Models.ChallengeGoals
{
    public class Frequency
    {
        public int Id { get; set; }

        [Required] public string Type { get; set; }

        public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    }
}
