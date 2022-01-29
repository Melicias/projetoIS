using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminConsole.Models
{
    public class Admin
    {
        public int id { get; set; }
        public String nome { get; set; }
        public String email { get; set; }
        public String password { get; set; }
        public int is_active { get; set; }
        public String accessToken { get; set; }

        public override string ToString()
        {
            return "(" + (is_active == 0 ? "Disable" : "Enable") + ") " + id + " - " + email ;
        }
    }
}