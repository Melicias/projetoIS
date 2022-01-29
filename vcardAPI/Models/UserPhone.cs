using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vcardAPI.Models
{
    public class UserPhone
    {
        public User user { get; set; }
        public Phone phone { get; set; }
    }
}