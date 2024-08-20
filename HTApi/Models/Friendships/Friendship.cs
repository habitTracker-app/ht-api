using HTAPI.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTAPI.Models.Friendships
{
    public class Friendship
    {
        public Guid Id { get; set; }

        [ForeignKey("Requester")]
        public string RequesterId { get; set; }
        public virtual User Requester { get; set; }

        [ForeignKey("Target")]
        public string TargetId { get; set; }
        public virtual User Target { get; set; }

        [ForeignKey("Status")]
        public int StatusId { get; set; }
        public virtual FriendshipStatus Status { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; }

    }
}
