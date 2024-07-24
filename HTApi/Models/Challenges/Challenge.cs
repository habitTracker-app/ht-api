using HTAPI.Models.ChallengeGoals;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTAPI.Models.Challenges
{
    public class Challenge
    {
        public Guid Id { get; set; }

        [Required] public string Title { get; set; }
        public string? Description { get; set; }


        [ForeignKey("Owner")]
        public string OwnerId { get; set; }
        public virtual User Owner { get; set; }

        
        public ICollection<User> Participants { get; set; } = new List<User>();


        [ForeignKey("Category")] [Required]
        public Guid CategoryId { get; set; }
        public virtual ChallengeCategory Category { get; set; }


        [Required] [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)] public DateTime? EndDate { get; set; }

        [Required]
        public bool Active { get; set; } = true;
    }
}
