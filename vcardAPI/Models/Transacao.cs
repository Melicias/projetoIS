using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace vcardAPI.Models
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
        public static string ACCESSTOKEN = "$HASH|V1$10000$axW0Rk4zXB3JNf67x5bKIPIMhNO6Ff0+BWKcfMtVAb8ZBw8d";
        public string id_vcard { get; set; }
        public TiposTransacao tipoTransacao { get; set; }
        public string phone_transaction { get; set; }
        public float montante { get; set; }
        public DateTime data { get; set; }
        public int id_category { get; set; }
        public string tipopayment { get; set; }
        public string payment_reference { get; set; }
        public int percentage { get; set; }
        public string access_token { get; set; }
        public string code { get; set; }
    }
}