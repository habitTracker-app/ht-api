namespace HTApi.Models.ActionModels
{
    public class RemoveFriendFromChallenge
    {
        public int UserUuid { get; set; }
        public int TargetUuid { get; set; }
        public string ChallengeId { get; set; }
    }
}
