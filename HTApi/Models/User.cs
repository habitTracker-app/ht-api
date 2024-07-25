using HTAPI.Data;
using HTAPI.Models.DemographicData;
using HTAPI.Models.Friendships;
using HTAPI.Models.Challenges;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTAPI.Models
{
    public class User : IdentityUser
    {
        [Required] public int UUID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string FName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string LName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required] public bool UserActive { get; set; }

        [Required] public bool AcceptedTerms { get; set; }

        [Required][DataType(DataType.DateTime)] public DateTime CreatedAt { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime UpdatedAt { get; set; }

        [Required]
        [ForeignKey("Country")]
        public int CountryId { get; set; }
        public virtual Country Country { get; set; }

        [Required]
        [ForeignKey("Gender")]
        public int GenderId { get; set; }
        public virtual Gender Gender { get; set; }

        public ICollection<Friendship> Friendships { get; set; } = new List<Friendship>();

        public ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();

        public ICollection<Progress> Progresses { get; set; } = new List<Progress>();

        public void SetUID(AppDbContext db)
        {
            if (!db.Users.Any() || db.Users.ToList().Count() == 0)
            {
                UUID = 10001;
            }
            else
            {
                int maxId = db.Users.Max(u => u.UUID);
                this.UUID = maxId + 1;
            }

        }
    }
}
