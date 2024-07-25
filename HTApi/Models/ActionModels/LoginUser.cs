using System.ComponentModel.DataAnnotations;

namespace HTApi.Models.ActionModels
{
    public class LoginUser
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool RememberMe { get; set; } = false;
    }
}
