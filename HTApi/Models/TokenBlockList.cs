using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HTApi.Models
{
    public class TokenBlockList
    {
        [Key]
        [Required]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InactivatedAt { get; set; }
    }
}
