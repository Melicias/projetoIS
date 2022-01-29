using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Banco1.Models
{
    public class Vcard
    {
        public int id { get; set; }
        public string phone_number { get; set; }
        public double balance{get; set; }
        public string code {get; set;}
    }
}