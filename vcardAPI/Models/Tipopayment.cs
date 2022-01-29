using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vcardAPI.Models
{
    public class Tipopayment
    {
        public int id { get; set; }
        public String code { get; set; }
        public String name { get; set; }
        public bool deleted_at { get; set; }
    }
}