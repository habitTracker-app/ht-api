using System.ComponentModel.DataAnnotations;

namespace HTApi.Models.ActionModels
{
    public class UpdateUserInfo
    {
        public string Email {  get; set; }
        public string FName { get; set; }
        public string LName { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public int CountryId { get; set; }

        public int GenderId { get; set; }
    }
}
