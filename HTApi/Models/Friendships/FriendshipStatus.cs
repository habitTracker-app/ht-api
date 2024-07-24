using System.ComponentModel.DataAnnotations;

namespace HTAPI.Models.Friendships
{
    public class FriendshipStatus
    {
        public int Id { get; set; }

        [Required]
        public string Status { get; set; }

        public ICollection<Friendship> Friendships { get; set; } = new List<Friendship>();
    }
}
