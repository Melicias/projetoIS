using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using uPLibrary.Networking.M2Mqtt;
using vcardAPI.Models;

namespace vcardAPI.Controllers
{
    [RoutePrefix("api/transacao")]
    public class TransacoesController : ApiController
    {
        MqttClient m_cClient = new MqttClient("127.0.0.1");
        string connectionString = Properties.Settings.Default.ConnStr;

        // This method send a resquest for bank to make transactions
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> postTransacaoAsync(Transacao transacao)
        {
            string erro = checkTransacao(transacao);
            if (erro == null) {
                try
                {
                    HttpContext httpContext = HttpContext.Current;
                    string authHeader = httpContext.Request.Headers["Authorization"];
                    SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();
                    //validate vcard
                    SqlCommand command = new SqlCommand("Select * From Vcards where Phone_number= @id_vcard", conn);
                    command.Parameters.AddWithValue("@id_vcard", transacao.id_vcard);
                    SqlDataReader reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        reader.Close();
                        conn.Close();
                        return BadRequest("Your Vcard doesn't exist dont exist");
                    }
                    reader.Close();
                    //validate category_type
                    command = new SqlCommand("Select * From Categorias where Id = @categoria_id and deleted_at is NULL", conn);
                    command.Parameters.AddWithValue("@categoria_id", transacao.id_category);
                    reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        reader.Close();
                        conn.Close();
                        return BadRequest("This category is not avaible or doesn't exist");
                    }
                    reader.Close();

                    transacao.tipopayment = transacao.tipopayment.ToUpper();
                    command = new SqlCommand("Select * From TipoPayment where Code = @tipoPayment and Deleted_at is NULL", conn);
                    command.Parameters.AddWithValue("@tipoPayment", transacao.tipopayment);
                    reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        reader.Close();
                        conn.Close();
                        return BadRequest("This Payment type is not avaible or doesn't exist");
                    }
                    reader.Close();
                    string vcardApiIdUser = "";
                    if (transacao.tipopayment == "VCARD")
                    {
                        command = new SqlCommand("Select * From Vcards where Phone_number= @id_vcard", conn);
                        command.Parameters.AddWithValue("@id_vcard", transacao.phone_transaction);
                        reader = command.ExecuteReader();
                        if (!reader.Read())
                        {
                            reader.Close();
                            conn.Close();
                            return BadRequest("This Vcard you are trying to send the money too, doesn't exist");
                        }
                        reader.Close();
                    }
                   
                    if (transacao.tipopayment == "USER")
                    {
                        command = new SqlCommand("Select * From Users where Id = @idUser", conn);
                        command.Parameters.AddWithValue("@idUser", transacao.payment_reference);
                        reader = command.ExecuteReader();
                        if (!reader.Read())
                        {
                            reader.Close();
                            conn.Close();
                            return BadRequest("This User account doesn't exist");
                        }
                        vcardApiIdUser = transacao.payment_reference;
                        transacao.payment_reference = (int)reader["Id_user"] + "";
                        reader.Close();
                    }
                    int idBanco = 0;
                    //get ip from vcard
                    string email = "";
                    command = new SqlCommand("Select b.*,u.email From Bancos b join Vcards v On(b.Id=v.Id_banco) join Users u On(b.Id=u.Id_banco) where v.Phone_number = @id", conn);
                    command.Parameters.AddWithValue("@id", transacao.id_vcard);
                    reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        idBanco= (int)reader["Id"];
                        email = (string)reader["email"];
                        String ip1 = @"" + (string)reader["Ip"] + "";
                        transacao.percentage = (int)reader["Percentagem"];
                        reader.Close();
                        if (ip1 == null)
                        {
                            return BadRequest("Some error with the bank");
                        }
                        String ip2 = null;
                        int idBanco2 = 0;
                        if (transacao.tipopayment == "VCARD")
                        {
                            //get ip fromm the vcard we are sending the money too
                            command = new SqlCommand("Select b.* From Bancos b join Vcards v On(b.Id=v.Id_banco) where v.Phone_number = @id", conn);
                            command.Parameters.AddWithValue("@id", transacao.phone_transaction);
                            reader = command.ExecuteReader();
                            if (reader.Read())
                            {
                                idBanco2 = (int)reader["Id"];
                                ip2 = @"" + (string)reader["Ip"] + "";
                                if (ip2 == null)
                                {
                                    return BadRequest("Some error with the bank that you are trying to send money");
                                }
                            }
                        }
                        //verificar esta query
                       
                        if (transacao.tipopayment == "USER")
                        {
                            //get ip fromm the vcard we are sending the money too
                            command = new SqlCommand("Select b.* From Bancos b join Users u On(b.Id=u.Id_banco) where u.Id = @idUser", conn);
                            command.Parameters.AddWithValue("@idUser", vcardApiIdUser);
                            reader = command.ExecuteReader();
                            if (reader.Read())
                            {
                                
                                ip2 = @"" + (string)reader["Ip"] + "";
                                if (ip2 == null)
                                {
                                    return BadRequest("Some error with the bank that you are trying to send money");
                                }
                            }
                            transacao.phone_transaction = "";
                        }
                        
                        try
                        {
                            Transacao transacaoCopia = transacao;

                            //fazer o debito da conta
                            var client = new RestSharp.RestClient(ip1);
                            if(transacao.tipopayment == "VCARD" || transacao.tipopayment == "USER")
                            {
                                //vai dar erro e n faz o pedido a api caso o ip esteja mal
                                var client2 = new RestSharp.RestClient(ip2);
                            } 
                            //send a request for the first bank with transaction type Debit
                            var request = new RestSharp.RestRequest("api/transacao", RestSharp.Method.POST);
                            request.AddHeader("Authorization", authHeader);
                            request.AddHeader("Accept", "application/json");
                            transacao.tipoTransacao = TiposTransacao.D;
                            transacao.data = DateTime.Now;
                            request.AddJsonBody(transacao);
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;

                            //converter para transaction para depois enviar isso
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                

                                //fazer o credito a outra conta
                                if (transacao.tipopayment.ToUpper() == "VCARD" || transacao.tipopayment.ToUpper() == "USER")
                                {
                                    //testar este bocado de codigo
                                    var client2 = new RestSharp.RestClient(ip2);
                                    var request2 = new RestSharp.RestRequest("api/transacao/credit", RestSharp.Method.POST);
                                    request2.AddHeader("Authorization", authHeader);
                                    request2.AddHeader("Accept", "application/json");
                                    string idvcard = transacao.id_vcard;
                                    transacao.id_vcard = transacao.phone_transaction;
                                    transacao.phone_transaction = idvcard;
                                    transacao.tipoTransacao = TiposTransacao.C;
                                    //send a static token with the request
                                    transacao.access_token = Transacao.ACCESSTOKEN;
                                    request2.AddJsonBody(transacao);
                                    RestSharp.IRestResponse response2 = client2.Execute(request2);
                                    String content2 = response2.Content;

                                    //send a request for the first bank if the sencond dont make in success
                                    if (response2.StatusCode != HttpStatusCode.OK)
                                    {
                                        
                                        var request3 = new RestRequest("api/transacao/crediterror", RestSharp.Method.POST);
                                        request3.AddHeader("Authorization", authHeader);
                                        request3.AddHeader("Accept", "application/json");
                                        transacao.tipoTransacao = TiposTransacao.C;
                                        string id_vcard = transacao.id_vcard;
                                        transacao.id_vcard = transacao.phone_transaction;
                                        transacao.phone_transaction = id_vcard;
                                        request3.AddJsonBody(transacao);
                                        IRestResponse response3 = client.Execute(request3);
                                        String content3 = response3.Content;
                                        if (response.StatusCode == HttpStatusCode.OK)
                                        {
                                            return Ok("There was some type of problem when giving the money to the person");
                                        }
                                        return BadRequest(content3);
                                    }
                                    else
                                    {
                                        reader.Close();
                                        try
                                        {
                                            m_cClient.Connect(Guid.NewGuid().ToString());
                                        }
                                        catch(Exception e)
                                        {

                                        }
                                        if (transacaoCopia.tipopayment.ToUpper() == "VCARD")
                                        {
                                            var request3 = new RestSharp.RestRequest("api/phone/" + transacao.id_vcard, RestSharp.Method.GET);
                                            request3.AddHeader("Authorization", authHeader);
                                            request3.AddHeader("Accept", "application/json");
                                            IRestResponse response3 = client2.Execute(request3);
                                            String content3 = response3.Content;
                                            Phone phone = client2.Execute<Phone>(request3).Data;
                                            command = new SqlCommand("Select * From Users  where Id_User=@id_user And Id_banco=@id_banco", conn);
                                            command.Parameters.AddWithValue("@id_user", phone.id_user);
                                            command.Parameters.AddWithValue("@id_banco", idBanco2);
                                            reader = command.ExecuteReader();
                                            if (reader.Read())
                                            {
                                                try
                                                {
                                                    //send a notification to the channel with ne name User ID
                                                    m_cClient.Publish(((int)reader["Id"]) + "", Encoding.UTF8.GetBytes("You have recive " + transacaoCopia.montante + "€ on your account from " + transacaoCopia.tipopayment + " With the number " + transacaoCopia.phone_transaction));
                                                }
                                                catch (Exception e)
                                                {

                                                }
                                            }
                                            reader.Close();
                                        }
                                        if(transacaoCopia.tipopayment.ToUpper() != "VCARD")
                                        {
                                            string id_vcard = transacaoCopia.id_vcard;
                                            transacaoCopia.id_vcard = transacaoCopia.phone_transaction;
                                            transacaoCopia.phone_transaction = id_vcard;
                                            try
                                            {
                                                //send a notification to the channel with ne name User ID
                                                m_cClient.Publish(vcardApiIdUser, Encoding.UTF8.GetBytes("You have recive " + transacaoCopia.montante + "€ on your account from " + transacaoCopia.tipopayment + " with email " + email));
                                            }
                                            catch (Exception e)
                                            {

                                            }
                                        }
                                        transacaoCopia.access_token = "";
                                        return Ok(transacaoCopia);
                                    }
                                }
                                else
                                {
                                
                                      
                                        var request3 = new RestSharp.RestRequest("api/phone/" + transacao.id_vcard, RestSharp.Method.GET);
                                        request3.AddHeader("Authorization", authHeader);
                                        request3.AddHeader("Accept", "application/json");
                                        IRestResponse response3 = client.Execute(request3);
                                        String content3 = response3.Content;
                                        Phone phone = client.Execute<Phone>(request3).Data;
                                        command = new SqlCommand("Select * From Users  where Id_User=@id_user And Id_banco=@id_banco", conn);
                                        command.Parameters.AddWithValue("@id_user", phone.id_user);
                                        command.Parameters.AddWithValue("@id_banco", idBanco);
                                        reader = command.ExecuteReader();
                                        if (reader.Read())
                                        {
                                            try
                                            {
                                            m_cClient.Connect(Guid.NewGuid().ToString());
                                            //send a notification to the channel with ne name User ID
                                            m_cClient.Publish(((int)reader["Id"]) + "", Encoding.UTF8.GetBytes("You have recive " + (transacaoCopia.percentage * 0.01) * transacaoCopia.montante + "€ on your account from using our API"));
                                            }catch(Exception e)
                                        {

                                        }
                                        }
                                        return Ok(transacao);
                                    
                                }
                                return Ok(transacao);
                            }
                            conn.Close();
                            return BadRequest(content);
                        }
                        catch (System.Net.WebException ex)
                        {
                            return BadRequest("There is some problem with the banks internet connection");
                        }
                    }
                }
                catch (Exception ee)
                {
                    return BadRequest();
                }
            }
            return BadRequest(erro);
        }
           
        //this method verify if a transaction is valid
        private string checkTransacao(Transacao transacao)
        {
            if(transacao.id_vcard.Length == 9)
            {
                if (transacao.tipopayment == null)
                {
                    return "The payment type is empty";
                }
                if (transacao.montante > 0)
                {
                    if (transacao.code != null)
                    {
                        if (transacao.tipopayment.ToUpper() == "VCARD")
                        {
                            if (transacao.phone_transaction.Length != 9)
                            {
                                return "The phone number you are sending it too is not valid";
                            }
                        }
                        else
                        {
                            if (transacao.payment_reference != null)
                            {
                                //sucesso
                                return null;
                            }
                            else
                            {
                                return "There is no payment reference";
                            }
                        }
                    }
                    else
                    {
                        return "The code is needed for confirmation";
                    }
                }
                else
                {
                    return "The amount you are sending need to be above 0";
                }
            }
            else
            {
                return "The vcard number is not valid";
            }
            //sucesso
            return null;
        }

        [Route("extern")]
        //this method send a resquest to the bank to make a credit transaction from  external 
        [HttpPost]
        public async Task<IHttpActionResult> postTransacaoExtern(Transacao transacao)
        {
            HttpContext httpContext = HttpContext.Current;
            string authHeader = httpContext.Request.Headers["Authorization"];
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            try
            {
                if (transacao.tipopayment.ToUpper() == "VCARD" || transacao.tipopayment.ToUpper() == "USER")
                {
                    return BadRequest("Wrong route");
                }
                if (transacao.id_category == 0)
                {
                    return BadRequest("Must have a category");
                }
                if (transacao.phone_transaction.Length != 9)
                {
                    return BadRequest("The phone number you are sending it too is not valid");
                }
                SqlCommand command = new SqlCommand("Select * From Vcards where Phone_number= @id_vcard", conn);
                command.Parameters.AddWithValue("@id_vcard", transacao.phone_transaction);
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    reader.Close();
                    conn.Close();
                    return BadRequest("This Vcard you are trying to send the money too, doesn't exist");
                }
                reader.Close();
                command = new SqlCommand("Select b.* From Bancos b join Vcards v On(b.Id=v.Id_banco) where v.Phone_number = @id", conn);
                command.Parameters.AddWithValue("@id", transacao.phone_transaction);
                reader = command.ExecuteReader();
                string ip1 = null;
                int idBanco = 0;
                if (reader.Read())
                {
                    idBanco = (int)reader["Id"];
                    ip1 = @"" + (string)reader["Ip"] + "";
                    if (ip1 == null)
                    {
                        return BadRequest("Some error with the bank that you are trying to send money");
                    }
                }
                reader.Close();

                var client = new RestSharp.RestClient(ip1);
                var request = new RestSharp.RestRequest("api/transacao/credit", RestSharp.Method.POST);
                request.AddHeader("Authorization", authHeader);
                request.AddHeader("Accept", "application/json");
                transacao.id_vcard = "";
                transacao.tipoTransacao = TiposTransacao.C;
                transacao.phone_transaction = transacao.payment_reference;
                transacao.access_token = Transacao.ACCESSTOKEN;
                request.AddJsonBody(transacao);
                RestSharp.IRestResponse response = client.Execute(request);
                String content = response.Content;
                var request3 = new RestSharp.RestRequest("api/phone/" + transacao.payment_reference, RestSharp.Method.GET);
                request3.AddHeader("Authorization", authHeader);
                request3.AddHeader("Accept", "application/json");
                IRestResponse response3 = client.Execute(request3);
                String content3 = response3.Content;
                Phone phone = client.Execute<Phone>(request3).Data;
                command = new SqlCommand("Select * From Users  where Id_User=@id_user And Id_banco=@id_banco", conn);
                command.Parameters.AddWithValue("@id_user", phone.id_user);
                command.Parameters.AddWithValue("@id_banco", idBanco);
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    m_cClient.Connect(Guid.NewGuid().ToString());
                    m_cClient.Publish(((int)reader["Id"]) + "", Encoding.UTF8.GetBytes("You have recive " + transacao.montante + "€ on your account from "+ transacao.tipopayment));
                }
                reader.Close();
                return Ok(content);
            }catch(Exception e)
            {
                return BadRequest();
            }
        }

        //this method send a request and recive all transactions from a phone_number
        [Route("{phone_number}")]
        public IHttpActionResult getTransacoes(string phone_number)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select b.* From Bancos b Join Vcards v On b.Id=v.Id_banco where v.Phone_number = @id ", conn);
                command.Parameters.AddWithValue("@id", phone_number);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String ip = @"" + (string)reader["Ip"] + "";
                    if (ip != null)
                    {
                        reader.Close();
                        conn.Close();
                        var client = new RestSharp.RestClient(ip);
                        var request = new RestSharp.RestRequest($"api/transacao/{phone_number}" , RestSharp.Method.GET);
                        request.AddHeader("Authorization", authHeader);
                        RestSharp.IRestResponse response = client.Execute(request);
                        String content = response.Content;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            List<Transacao> transacaos = client.Execute<List<Transacao>>(request).Data;
                            return Ok(transacaos);
                        }
                        return BadRequest(content);
                    }
                }
            }
            catch (Exception ee)
            {
                return BadRequest("There is some problem with this bank url");
            }

            return BadRequest();
        }

        //this method send a request and recive all transactions from a User
        [Route("byuser/{unique_id_user}")]
        public IHttpActionResult getTransacoesByUser(string unique_id_user)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select Ip From Bancos b Join Users u On b.Id=u.Id_banco where u.Id = @id ", conn);
                command.Parameters.AddWithValue("@id", unique_id_user);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String ip = @"" + (string)reader["Ip"] + "";
                    if (ip != null)
                    {
                        reader.Close();
                        conn.Close();
                        var client = new RestSharp.RestClient(ip);
                        var request = new RestSharp.RestRequest($"api/transacao/byiduser", RestSharp.Method.GET);
                        request.AddHeader("Authorization", authHeader);
                        RestSharp.IRestResponse response = client.Execute(request);
                        String content = response.Content;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            List<Transacao> transacaos = client.Execute<List<Transacao>>(request).Data;
                            return Ok(transacaos);
                        }
                        return BadRequest(content);
                    }
                }
                else
                {
                    return BadRequest("User not found");
                }
            }
            catch (Exception ee)
            {
                return BadRequest("There is some problem with this bank url");
            }
            return BadRequest();
        }

        //this method send a request and recive all transactions and only admins can access to this method
        [Route("all")]
        [Authorize]
        public IHttpActionResult getTransacoesAdmin()
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select Ip, Name From Bancos where Deleted_at is NULL", conn);
                SqlDataReader reader = command.ExecuteReader();
                List<nameTransacoes> transacoes = new List<nameTransacoes>();
                while (reader.Read())
                {
                    string ip = @"" + (string)reader["Ip"] + "";
                    string name = (string)reader["Name"];
                    if (ip != null)
                    {
                        try
                        {
                            var client = new RestSharp.RestClient(ip);
                            var request = new RestSharp.RestRequest($"api/transacao", RestSharp.Method.GET);
                            request.AddHeader("Accept", "application/json");
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                List<Transacao> transacoesVindas = client.Execute<List<Transacao>>(request).Data;
                                transacoes.Add(new nameTransacoes { name = name, transacoes = transacoesVindas });
                            }
                        }catch(Exception e)
                        {

                        }
                    }
                }
                reader.Close();
                conn.Close();
                return Ok(transacoes);
            }
            catch (Exception ee)
            {
                return BadRequest("There is some problem with this bank url");
            }
            return BadRequest();
        }

        //this method send a request and recive all transactions from a bank and only admins can access to this method
        [Route("bank/{bankId}")]
        [Authorize]
        public IHttpActionResult getTransacoesByBankAdmin(int bankId)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select Ip, Name From Bancos where Id = @bankId AND Deleted_at is NULL", conn);
                command.Parameters.AddWithValue("@bankId", bankId);
                SqlDataReader reader = command.ExecuteReader();
                List<nameTransacoes> transacoesBanco = new List<nameTransacoes>();

                if (!reader.Read())
                {
                    reader.Close();
                    conn.Close();
                    return BadRequest("This Bank is not avaible or doesn't exist");
                }
                string ip = @"" + (string)reader["Ip"] + "";
                string name = (string)reader["Name"];
                reader.Close();
      
                if (ip != null)
                {
                    try
                    {
                        var client = new RestSharp.RestClient(ip);
                        var request = new RestSharp.RestRequest($"api/transacao", RestSharp.Method.GET);
                        request.AddHeader("Accept", "application/json");
                        RestSharp.IRestResponse response = client.Execute(request);
                        String content = response.Content;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            List<Transacao> transacoesVindas = client.Execute<List<Transacao>>(request).Data;
                            transacoesBanco.Add(new nameTransacoes { name = name, transacoes = transacoesVindas });
                        }
                    }
                    catch (Exception e)
                    {
                        return BadRequest("There is some problem with this bank url");
                    }
                }
                conn.Close();
                return Ok(transacoesBanco);
            }
            catch (Exception ee)
            {
                return BadRequest("There is some problem with this bank url");
            }
            return BadRequest();
        }

        //this method send a request and recive all transactions from a PaymentType and only admins can access to this method
        [Route("type/{type}")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult getTransacoesByTypeAdmin(string type)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From TipoPayment where Code = @tipoPayment and Deleted_at is NULL", conn);
                command.Parameters.AddWithValue("@tipoPayment", type);
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    reader.Close();
                    conn.Close();
                    return BadRequest("This Payment type is not avaible or doesn't exist");
                }
                reader.Close();
                 command = new SqlCommand("Select Ip, Name From Bancos where Deleted_at is NULL", conn);
                 reader = command.ExecuteReader();
                List<nameTransacoes> transacoesBanco = new List<nameTransacoes>();
                while (reader.Read())
                {
                    string ip = @"" + (string)reader["Ip"] + "";
                    string name = (string)reader["Name"];
                    if (ip != null)
                    {
                        try
                        {
                            var client = new RestSharp.RestClient(ip);
                            var request = new RestSharp.RestRequest($"api/transacao/type/" + type, RestSharp.Method.GET);
                            request.AddHeader("Accept", "application/json");
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                List<Transacao> transacaos = client.Execute<List<Transacao>>(request).Data;
                                transacoesBanco.Add(new nameTransacoes { name = name, transacoes = transacaos});
                            }
                        }
                        catch (Exception e)
                        {
                            return BadRequest("There is some problem with this bank url");
                        }
                    }
                }
                reader.Close();
                conn.Close();
                return Ok(transacoesBanco);
            }
            catch (Exception ee)
            {
                return BadRequest("There is some problem with this bank url");
            }
            return BadRequest();
        }

        //this method send a request and recive all transactions from a credit or Debit and only admins can access to this method
        [Route("operationType/{type}")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult getTransacoesByOperationTypeAdmin(int type)
        {
            if(type > 1 || type < 0)
            {
                return BadRequest("This type is not avaible");
            }
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select Ip, Name From Bancos where Deleted_at is NULL", conn);
                SqlDataReader reader = command.ExecuteReader();
                List<nameTransacoes> transacoesBanco = new List<nameTransacoes>();
                while (reader.Read())
                {
                    string ip = @"" + (string)reader["Ip"] + "";
                    string name = (string)reader["Name"];
                    if (ip != null)
                    {
                        try
                        {
                            var client = new RestSharp.RestClient(ip);
                            var request = new RestSharp.RestRequest($"api/transacao/operationType/" + type, RestSharp.Method.GET);
                            request.AddHeader("Accept", "application/json");
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                List<Transacao> transacaos = client.Execute<List<Transacao>>(request).Data;
                                transacoesBanco.Add(new nameTransacoes { name = name, transacoes = transacaos });
                            }
                        }
                        catch (Exception e)
                        {
                            return BadRequest("There is some problem with this bank url");
                        }
                    }
                }
                reader.Close();
                conn.Close();
                return Ok(transacoesBanco);
            }
            catch (Exception ee)
            {
                return BadRequest("There is some problem with this bank url");
            }
            return BadRequest();
        }
    }
}