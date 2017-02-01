using System.ComponentModel.DataAnnotations;

namespace Production_Electricite.Models
{
    public class User
    {
        public string _id { get; set; }
        [Required]
        public string login { get; set; }
        [Required]
        [StringLength(maximumLength:5, MinimumLength = 0)]
        public string password { get; set; }
        public int nbTentatives { get; set; }
    }
}