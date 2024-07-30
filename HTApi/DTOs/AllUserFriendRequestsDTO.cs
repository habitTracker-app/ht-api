using HTAPI.Models.Friendships;

namespace HTApi.DTOs
{
    public class AllUserFriendRequestsDTO
    {
        public List<FriendshipDTO> TargetFriendRequests { get; set; } = new List<FriendshipDTO>();
        public List<FriendshipDTO> RequesterFriendRequests { get; set; } = new List<FriendshipDTO>();

        public int TotalFriendRequests { get; set; }
        public int TotalTargetRequestCount { get; set; }
        public int TotalRequesterCount { get; set;}

        public AllUserFriendRequestsDTO(List<Friendship> rReqs, List<Friendship> tReqs)
        {
            List<FriendshipDTO> tDTOs = new List<FriendshipDTO>();
            foreach (var tReq in tReqs)
            {
                tDTOs.Add(new FriendshipDTO(tReq));
            }
            List<FriendshipDTO> rDTOs = new List<FriendshipDTO>();
            foreach (var rReq in rReqs)
            {
                rDTOs.Add(new FriendshipDTO(rReq));
            }

            this.RequesterFriendRequests = rDTOs;
            this.TargetFriendRequests = tDTOs;
            this.TotalRequesterCount = rDTOs.Count;
            this.TotalFriendRequests = tDTOs.Count + rDTOs.Count;
            this.TotalTargetRequestCount = tDTOs.Count;
        }
    }

}
