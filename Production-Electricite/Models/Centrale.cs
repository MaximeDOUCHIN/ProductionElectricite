using System;
using System.ComponentModel.DataAnnotations;

namespace Production_Electricite.Models
{
    public class Centrale
    {
        public enum TypeCentrale {
            Eolien,
            Solaire,
            Geothermique,
            Nucleaire,
            Hydrolique,
            Charbon
        };

        public string _id { get; set; }
        public int version { get; set; }
        public string userId { get; set; }
        [Required]
        public string reference { get; set; }
        [Required]
        [EnumDataType(typeof(TypeCentrale))]
        public object type { get; set; }
        [Required]
        [Range(0.001, Double.MaxValue)]
        public double capacite { get; set; }
        public double stock { get; set; }
        public DateTime lastModified { get; set; }
    }
}