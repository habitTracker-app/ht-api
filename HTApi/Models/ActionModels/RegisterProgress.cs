using System.ComponentModel.DataAnnotations;

namespace HTApi.Models.ActionModels
{
    public class RegisterProgress
    {
        [Required]
        public string GoalId { get; set; }
        [Required]
        public float ProgressAmount { get; set; }
    }
}
