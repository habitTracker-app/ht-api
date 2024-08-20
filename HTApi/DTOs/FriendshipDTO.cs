using HTAPI.Models;
using HTAPI.Models.Friendships;
using System.Text.Json;

namespace HTApi.DTOs
{
    public class FriendshipDTO
    {
        public Guid FriendshipId { get; set; }
        public int RequesterUUID { get; set; }
        public int TargetUUID { get; set; }

        public int FriendshipStatusId { get; set; }
        public string FriendshipStatus { get; set; }

        public FriendshipDTO(Friendship f) {
            this.FriendshipId = f.Id;
            Console.WriteLine(JsonSerializer.Serialize(f));
            this.RequesterUUID = f.Requester.UUID;
            this.TargetUUID = f.Target.UUID;
            this.FriendshipStatusId = f.Status.Id;
            this.FriendshipStatus = f.Status.Status;
        }

    }
}
