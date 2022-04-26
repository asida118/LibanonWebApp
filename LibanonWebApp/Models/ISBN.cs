using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibanonWebApp.Models
{
    public class ISBN
    {
        public int ISBNId { get; set; }
        public string ISBNCode { get; set; }
        public double RatingScore { get; set; } = 0;
        public int RatingNumber { get; set; } = 0;

        public virtual Book Book { get; set; }
    }
}