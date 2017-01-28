using System;
using System.ComponentModel.DataAnnotations;

namespace Production_Electricite.Models
{
    public class Stock
    {
        public string _id { get; set; }
        public string idCentrale { get; set; }
        public double quantite { get; set; }
        public DateTime dateCreation { get; set; }
    }

    public class Refill
    {
        [Required]
        public string reference { get; set; }
        [Required]
        public double quantite { get; set; }
    }

}