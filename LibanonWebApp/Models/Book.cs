using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibanonWebApp.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Published { get; set; }
        public string Summary { get; set; }
        public bool? Status { get; set; }
        public string BrwEmail { get; set; }
        public string BrwName { get; set; }
        public string BrwPhone { get; set; }
        public string BrwNote { get; set; }
        public virtual ISBN ISBN { get; set; }
        public virtual Owner Owner { get; set; }
        public virtual Confirmation Confirmation { get; set; }

    }
}