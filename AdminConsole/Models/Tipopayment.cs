using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminConsole.Models
{
    public class Tipopayment
    {
        public int id { get; set; }
        public String code { get; set; }
        public String name { get; set; }
        public bool deleted_at { get; set; }

        public override string ToString()
        {
            return (deleted_at ? "Disabled - " : "") + "" + code + " ("+name+")";
        }
    }
}