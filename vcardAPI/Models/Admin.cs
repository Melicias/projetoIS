using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vcardAPI.Models
{
    public class Admin
    {
        public int id { get; set; }
        public String nome { get; set; }
        public String email { get; set; }
        public String password { get; set; }
        public int is_active { get; set; }
    }
}