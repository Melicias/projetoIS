using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Banco1.Models
{
    public class Phone
    {
        public int id_user { get; set; }
        public string phone_number { get; set; }
        public string code { get; set; }
        public int vcard { get; set; }
    }
}