using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;

namespace Production_Electricite.Models
{
    public class CampusSupInfo
    {
        public string _id { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public int CodePostal { get; set; }
    }    
}