using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HTAPI.Models.DemographicData
{
    public class Gender
    {
        public int Id { get; set; }

        [Required] public string Name { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string FlagEmoji { get; set; }


        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
