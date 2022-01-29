using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Banco1.Models
{
    public enum TiposTransacao
    {
        C,
        D
         
    }
    public class Transacao
    {
        private static string ACCESSTOKENDECRYPTED = "ThisIsASecretKey";
        public int id { get; set; }
        public string id_vcard { get; set; }

        public TiposTransacao tipoTransacao { get; set; }
        public float montante { get; set; }

        public string phone_transaction { get; set; }
        public DateTime data { get; set; }
        public int id_category { get; set; }
        public string tipopayment { get; set; }
        public string payment_reference { get; set; }
        public int percentage { get; set; }
        public string code { get; set; }
        public string access_token { get; set; }

        public bool checkAccessToken()
        {
            return SecurePasswordHasher.Verify(ACCESSTOKENDECRYPTED, access_token);
        }
    }
}