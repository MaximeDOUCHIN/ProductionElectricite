using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Production_Electricite.Controllers
{
    public class User
    {
        public string _id { get; set; }
        [Required]
        public string login { get; set; }
        [StringLength(maximumLength:5, MinimumLength = 0)]
        public string password { get; set; }
        public int nbTentatives { get; set; }
    }
}