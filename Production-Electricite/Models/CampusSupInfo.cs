using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace Production_Electricite.Models
{
    public class CampusSupInfo
    {
        public string _id { get; set; }
        [Required]
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        [Required]
        [Range(0, 99999)]
        public int CodePostal { get; set; }
    }    
}