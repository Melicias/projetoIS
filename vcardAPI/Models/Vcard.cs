using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vcardAPI.Models
{
    public class Vcard
    {
        public int Phone_number { get; set; }
        public int id_banco { get; set; }
        public double balance { get; set; }
        public string code { get; set; }
    }
}