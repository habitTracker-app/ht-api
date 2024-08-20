using HTAPI.Models;
using HTAPI.Models.Challenges;

namespace HTApi.DTOs
{
    public class Top3DTO
    {
        public List<ChallengeParticipantDTO> Participants { get; set; } = [];
        public Top3DTO(ICollection<User> participants, DateTime start, DateTime end)
        {
            if(participants.Count() == 1)
            {
                this._addParticipant(participants.First(), start, end);
            }
            else
            {
                List<User> top3 = this._getTop3(participants, start, end);

                foreach (User user in top3)
                {
                    this._addParticipant(user, start, end);
                }
            }
        }

        private void _addParticipant(User user, DateTime start, DateTime end)
        {
            Progress? latest = user.Progresses
                                    .Where(prog => prog.Date >= start && prog.Date <= end)
                                    .OrderBy(prog => prog.Date)
                                    .LastOrDefault();

            this.Participants.Add(new ChallengeParticipantDTO()
            {
                Name = user.FName + " " + user.LName,
                ProgressPercentage = latest != null ? latest.UpdatedProgressPercentage : 0,
                Score = latest != null ? latest.UpdatedScore : 0,
                PUID = user.UUID
            });
        }
        private List<User> _getTop3(ICollection<User> participants, DateTime start, DateTime end)
        {
            List<User> top3 = [];

            int maxTop3 = participants.Count >= 3 ? 3 : participants.Count;

            top3 = participants.OrderBy(p => p.Progresses
                                        .Where(prog => prog.Date >= start && prog.Date <= end)
                                        .OrderBy(prog => prog.UpdatedScore)
                                        .LastOrDefault()?
                                        .UpdatedScore)?.ToList().Slice(0, maxTop3) ?? [];

            return top3;
        }
    }
}
