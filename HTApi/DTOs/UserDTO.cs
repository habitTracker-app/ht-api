using HTAPI.Models.Challenges;
using HTAPI.Models.DemographicData;
using HTAPI.Models.Friendships;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HTAPI.Models;

namespace HTApi.DTOs
{
    public class UserDTO
    {
        public int UUID { get; set; }

        public string FName { get; set; }
        
        public string LName { get; set; }

        public DateTime BirthDate { get; set; }

        public bool UserActive { get; set; }

        public bool AcceptedTerms { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int CountryId { get; set; }

        public int GenderId { get; set; }

        public List<Guid> FriendshipsIds { get; set; }

        public List<Guid> ChallengesIds { get; set; }
        public List<Guid> ProgressesIds { get; set; }


        public UserDTO(User u)
        {
            this.UUID = u.UUID;
            this.FName = u.FName;
            this.LName = u.LName;
            this.BirthDate = u.BirthDate;
            this.UserActive = u.UserActive;
            this.AcceptedTerms = u.AcceptedTerms;
            this.CreatedAt = u.CreatedAt;
            this.UpdatedAt = u.UpdatedAt;
            this.CountryId = u.CountryId;
            this.GenderId = u.GenderId;
            this.FriendshipsIds = [];
            this.ChallengesIds = [];
            this.ProgressesIds = [];


            foreach (Friendship fs in u.Friendships)
            {
                this.FriendshipsIds.Add(fs.Id);
            }
            foreach (Challenge c in u.Challenges)
            {
                this.ChallengesIds.Add(c.Id);
            }
            foreach (Progress p in u.Progresses)
            {
                this.ProgressesIds.Add(p.Id);
            }
        }
    }
}
