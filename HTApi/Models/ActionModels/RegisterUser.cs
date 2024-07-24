using System.ComponentModel.DataAnnotations;

namespace HTAPI.Models.ActionModels
{
    public class RegisterUser
    {
        public string Email { get; set; }

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public DateTime BirthDate { get; set; }

        public string FName { get; set; }
        public string LName { get; set; }

        public int CountryId { get; set; }
        public int GenderId { get; set; }

        public bool TermsAccepted { get; set; }
    }
}
