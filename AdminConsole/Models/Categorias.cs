using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminConsole.Models
{
    public class Categorias
    {
        public int id { get; set; }
        public string nome { get; set; }
        public bool deleted_at { get; set; }

        public override string ToString()
        {
            return (deleted_at ? "Disabled - " : "") + "" + nome;
        }
    }
}