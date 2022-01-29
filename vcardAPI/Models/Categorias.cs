using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vcardAPI.Models
{
    public class Categorias
    {
        public int id { get; set; }
        public string nome { get; set; }
        public bool deleted_at { get; set; }
    }
}