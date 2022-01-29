﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminConsole.Models
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string accessToken { get; set; }
        public int idBanco { get; set; }

        public override string ToString()
        {
            return id + " - " + name + " ("+email+")";
        }
    }
}