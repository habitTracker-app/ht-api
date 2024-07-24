using System.ComponentModel.DataAnnotations;

namespace HTAPI.Models.Challenges
{
    public class ChallengeCategory
    {
        public Guid Id { get; set; }

        [Required] public string Name { get; set; }
        [Required] public string Description { get; set; }

        public ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();
    }
}
