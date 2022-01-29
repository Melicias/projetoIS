using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Banco1.Models
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public double balance { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string accessToken { get; set; }
        public int idBanco { get; set; }
    }
}