using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vcardAPI.Models
{
    public class Banco
    {
        public int id { get; set; }
        public String ip { get; set; }

        public String name { get; set; }

        public bool isDeleted { get; set; }

        public int percentagem { get; set; }

        public int max_debit_limit { get; set; }
    }
}