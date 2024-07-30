using System.ComponentModel.DataAnnotations;

namespace HTApi.Models.ActionModels
{
    public class AddFriend
    {
        [Required]
        public int RequesterUuid { get; set; }

        [Required]
        public int TargetUuid { get; set; }
    }
}
