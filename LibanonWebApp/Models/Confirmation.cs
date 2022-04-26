using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibanonWebApp.Models
{
    public class Confirmation
    {
        public int ConfirmationId { get; set; }
        public string OTP { get; set; }
        public bool ConfirmBorrow { get; set; } = false;
        public bool ConfirmLend { get; set; } = false;
        public virtual Book Book { get; set; }
    }
}