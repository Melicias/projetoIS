using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminConsole.Models
{
    public class nameTransacoes
    {
        public string name { get; set; }
        public List<Transacao> transacoes { get; set; }
    }
    public enum TiposTransacao
    {
        C,
        D

    }
    public class Transacao
    {
        public static string ACCESSTOKEN = "";
        public string id_vcard { get; set; }
        public TiposTransacao tipoTransacao { get; set; }
        public string phone_transaction { get; set; }
        public float montante { get; set; }
        public DateTime data { get; set; }
        public int id_category { get; set; }
        public string tipopayment { get; set; }
        public string payment_reference { get; set; }
        public string access_token { get; set; }
        public string code { get; set; }
    }
}