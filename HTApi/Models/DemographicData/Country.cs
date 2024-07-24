using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HTAPI.Models.DemographicData
{
    public class Country
    {
        public int Id { get; set; }

        [Required] public string Name { get; set; }

        [Required] public int CountryCode { get; set; }
        [Required] public string NativeName { get; set; }
        [Required] public string Abbreviation { get; set; }

        [Required] [DataType(DataType.Text)]
        public string FlagEmoji { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
