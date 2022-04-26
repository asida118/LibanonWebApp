using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibanonWebApp.Models
{
    public class Owner
    {
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerPhone { get; set; }

        public virtual Book Book { get; set; }
    }
}