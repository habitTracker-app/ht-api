using System.ComponentModel.DataAnnotations;

namespace HTApi.Models.ActionModels
{
    public class DeleteUser
    {
        [Required]
        public int uuid { get; set; }
        
        [Required]
        public string password { get; set; }
    }
}
