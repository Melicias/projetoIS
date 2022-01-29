using Banco1.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
namespace Banco1.Controllers
{
    [RoutePrefix("api/transacao")]
    public class TransacoesController : ApiController
    {
        // GET: Transacoes
        //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["BancoApp.Properties.Settings.ConnectionToDB"].ConnectionString;
        string connectionString = Properties.Settings.Default.ConnStr;

        //This method isnt used
        [Route("")]
        [HttpGet]
        public IHttpActionResult GetAllTransacoes()
        {
            List<Transacao> transacoes = new List<Transacao>();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Transacoes Where Delete_at IS NULL Order By Id  ", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transacao transacao = new Transacao
                    {
                        id = (int)reader["Id"],
                        id_vcard = (string)reader["Id_vcard"],
                        tipoTransacao = (TiposTransacao)Enum.Parse(typeof(TiposTransacao), (string)reader["TipoTransacao"]),
                        montante = (float)(double)reader["Montante"],
                        data = (DateTime)reader["Data"],
                        tipopayment = (string)reader["tipoPayment"],
                        id_category = (int)reader["id_category"],
                        phone_transaction = (string)reader["Phone_transaction"],
                        payment_reference = (string)reader["Payment_reference"]
                    };
                    transacoes.Add(transacao);
                }
                reader.Close();
                conn.Close();
                return Ok(transacoes);
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return null;
            }
            return BadRequest("Something happen");
        }

        [Route("{phone_number}")]
        [Authorize]
        
        public IHttpActionResult GetTransacaoFromUserPhone(string phone_number)
        {
            List<Transacao> transacoes = new List<Transacao>();
            SqlConnection conn = null;
            var identity = (ClaimsIdentity)User.Identity;
            int id_user = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand command = new SqlCommand("Select * from Phones Where Phone_number=@phone_number AND Deleted_at IS NULL",conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return BadRequest("This Phone doesnt exist");
                }

                if ((int)reader["Id_user"] != id_user)
                {
                    return BadRequest("This Vcard Doesnt belong to you");
                }
                reader.Close();
                command = new SqlCommand("Select * From Transacoes t Join Phones p On (t.Id_vcard=p.Phone_number) Where Id_vcard = @phone_number AND p.id_user=@id_user AND Deleted_at IS NULL  ", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                command.Parameters.AddWithValue("@id_user", id_user);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transacao transacao = new Transacao
                    {
                        id = (int)reader["Id"],
                        id_vcard = (string)reader["Id_vcard"],
                        tipoTransacao = (TiposTransacao)Enum.Parse(typeof(TiposTransacao), (string)reader["TipoTransacao"]),
                        montante = (float)(double)reader["Montante"],
                        data = (DateTime)reader["Data"],
                        tipopayment = (string)reader["tipoPayment"],
                        id_category = (int)reader["id_category"],
                        phone_transaction = (string)reader["Phone_transaction"],
                        payment_reference = (string)reader["Payment_reference"]
                    };
                    transacoes.Add(transacao);
                }
                reader.Close();
                conn.Close();
                return Ok(transacoes);

            }
            catch(Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }

        [Route("byiduser")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult GetTransacaoFromUser()
        {
            List<Transacao> transacoes = new List<Transacao>();
            SqlConnection conn = null;
            var identity = (ClaimsIdentity)User.Identity;
            int id_user = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                SqlCommand command = new SqlCommand("select * from Transacoes where id_vcard in (select v.Phone_number from Vcards v join Phones p on (p.Phone_number = v.Phone_number) where p.Id_user = @id)or(TipoPayment = 'USER' AND Payment_reference = @id AND TipoTransacao = 0); ", conn);
                command.Parameters.AddWithValue("@id", id_user);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transacao transacao = new Transacao
                    {
                        id = (int)reader["Id"],
                        id_vcard = (string)reader["Id_vcard"],
                        tipoTransacao = (TiposTransacao)Enum.Parse(typeof(TiposTransacao), (string)reader["TipoTransacao"]),
                        montante = (float)(double)reader["Montante"],
                        data = (DateTime)reader["Data"],
                        tipopayment = (string)reader["tipoPayment"],
                        id_category = (int)reader["id_category"],
                        phone_transaction = (string)reader["Phone_transaction"],
                        payment_reference = (string)reader["Payment_reference"]
                    };
                    transacoes.Add(transacao);
                }
                reader.Close();
                conn.Close();
                return Ok(transacoes);
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }

        [Route("operationType/{type}")]
        [HttpGet]
        public IHttpActionResult GetTranscationByOperationType(int type)
        {
            List<Transacao> transacoesPorTipoOperacao = new List<Transacao>();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM Transacoes WHERE TipoTransacao = @type AND Delete_at IS NULL", conn);
                command.Parameters.AddWithValue("@type", type);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transacao transacao = new Transacao
                    {
                        id = (int)reader["Id"],
                        id_vcard = (string)reader["Id_vcard"],
                        tipoTransacao = (TiposTransacao)Enum.Parse(typeof(TiposTransacao), (string)reader["TipoTransacao"]),
                        montante = (float)(double)reader["Montante"],
                        data = (DateTime)reader["Data"],
                        tipopayment = (string)reader["tipoPayment"],
                        id_category = (int)reader["id_category"],
                        phone_transaction = (string)reader["Phone_transaction"],
                        payment_reference = (string)reader["Payment_reference"]
                    };
                    transacoesPorTipoOperacao.Add(transacao);
                }
                reader.Close();
                conn.Close();
                return Ok(transacoesPorTipoOperacao);
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }

        [Route("type/{type}")]
        [HttpGet]
        public IHttpActionResult GetTranscationByType(string type)
        {
            List<Transacao> transacoesPorTipo = new List<Transacao>();
            SqlConnection conn = null;
            try
            {
                type = type.ToUpper();
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM Transacoes WHERE TipoPayment = @type AND Delete_at IS NULL", conn);
                command.Parameters.AddWithValue("@type", type);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transacao transacao = new Transacao
                    {
                        id = (int)reader["Id"],
                        id_vcard = (string)reader["Id_vcard"],
                        tipoTransacao = (TiposTransacao)Enum.Parse(typeof(TiposTransacao), (string)reader["TipoTransacao"]),
                        montante = (float)(double)reader["Montante"],
                        data = (DateTime)reader["Data"],
                        tipopayment = (string)reader["tipoPayment"],
                        id_category = (int)reader["id_category"],
                        phone_transaction = (string)reader["Phone_transaction"],
                        payment_reference = (string)reader["Payment_reference"]
                    };
                    transacoesPorTipo.Add(transacao);
                }
                reader.Close();
                conn.Close();
                return Ok(transacoesPorTipo);
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }

        [Route("")]
        [HttpPost]
        [Authorize]
        //this method create a transaction on the bank of type Debit
        public IHttpActionResult PostTransacao(Transacao transacao)
        {
            var identity = (ClaimsIdentity)User.Identity;
            int id_user = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            HttpContext httpContext = HttpContext.Current;
            string code = transacao.code;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            SqlDataReader reader;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;
            try
            {
                if (transacao.tipoTransacao != TiposTransacao.D)
                {
                    return BadRequest("Only debit transaction allowed");
                }
                command.CommandText = "Select * from Phones Where Phone_number=@phone_number AND Deleted_at IS NULL";
                command.Parameters.AddWithValue("@phone_number", transacao.id_vcard);
                reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return BadRequest("This Phone doesnt exist");
                }

                if (transacao.tipoTransacao == TiposTransacao.D && (int)reader["Id_user"] != id_user)
                {
                    return BadRequest("This Vcard Doesnt belong to you");
                }

                reader.Close();
                command.CommandText = "Select * from Vcards where Phone_number=@phone_number AND Deleted_at IS NULL";
                reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return BadRequest("This Vcard doesnt exist");
                }
                if (SecurePasswordHasher.Verify(code,(string) reader["Code"])){
                    return BadRequest("Wrong Code");
                }
                if (Convert.ToInt32(reader["Balance"]) < transacao.montante)
                {
                    return BadRequest("Dont have enought money");
                }
                //verificar se e vcard ou outro metodo com o D apenas, por isso aqui dentro
                double balance = (float)(double)(Convert.ToInt32(reader["Balance"]));
                command.CommandText = "Update Vcards Set Balance = @balance where Phone_number = @phone_number";
                command.Parameters.AddWithValue("@Balance", balance - transacao.montante);
                reader.Close();
                command.ExecuteNonQuery();

                if (transacao.tipopayment.ToUpper() != "VCARD" && transacao.tipopayment.ToUpper() != "USER")
                {
                    if (transacao.percentage != null) {
                        double precentageValue = (transacao.percentage * 0.01) * transacao.montante;
                        command.CommandText = "Update Vcards Set Balance = @balance2 where Phone_number = @phone_number";
                        command.Parameters.AddWithValue("@Balance2", (balance - transacao.montante) + precentageValue);
                        reader.Close();
                        command.ExecuteNonQuery();
                    }
                }
                if (transacao.tipopayment.ToUpper() == "VCARD")
                {
                    transacao.payment_reference = transacao.phone_transaction;
                }
                command.CommandText = "Insert INTO Transacoes(Id_vcard,TipoTransacao,Montante,Data,Id_category,Phone_transaction,TipoPayment,Payment_reference) values(@id_vcard,@tipoTransacao,@montante,@date,@id_category,@phone_transaction,@tipoPayment,@payment_reference) ";
                command.Parameters.AddWithValue("@tipoTransacao", transacao.tipoTransacao);
                command.Parameters.AddWithValue("@id_vcard", transacao.id_vcard);
                command.Parameters.AddWithValue("@montante", transacao.montante);
                command.Parameters.AddWithValue("@phone_transaction", transacao.phone_transaction == null? "": transacao.phone_transaction);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.Parameters.AddWithValue("@id_category", transacao.id_category);
                command.Parameters.AddWithValue("@tipoPayment", transacao.tipopayment);
                command.Parameters.AddWithValue("@payment_reference", transacao.payment_reference);
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
                return Ok(transacao);
            }
            catch (Exception e)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest();
            }
        }

        [Route("credit")]
        //this method create a transaction on the bank of type Credit
        [HttpPost]
        public IHttpActionResult PostTransacaoCredit(Transacao transacao)
        {
            HttpContext httpContext = HttpContext.Current;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            SqlDataReader reader;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;
            if (!transacao.checkAccessToken())
            {
                return BadRequest("You dont have permission for this operation");
            }
            try
            {
                if (transacao.tipoTransacao != TiposTransacao.C)
                {
                    return BadRequest("Only credit operation are allowed");
                }
                if (transacao.tipopayment == "VCARD")
                {
                    command.CommandText = "Select * from Phones Where Phone_number=@phone_number AND Deleted_at IS NULL";
                    command.Parameters.AddWithValue("@phone_number", transacao.id_vcard);
                    reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        return BadRequest("This Phone doesnt exist");
                    }
                    reader.Close();

                    command.CommandText = "Select * from Vcards where Phone_number=@phone_number AND Deleted_at IS NULL";
                    reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        return BadRequest("This Vcard doesnt exist");
                    }
                    //verificar se e vcard ou outro metodo com o D apenas, por isso aqui dentro
                    command.CommandText = "Update Vcards Set Balance = @balance where Phone_number = @phone_number";
                    command.Parameters.AddWithValue("@Balance", (float)(double)(Convert.ToInt32(reader["Balance"]) + transacao.montante));
                    reader.Close();
                    command.ExecuteNonQuery();
                    transacao.payment_reference = transacao.phone_transaction;
                }
                if (transacao.tipopayment == "USER")
                {
                    command.CommandText = "Select * from Users Where Id=@idUser AND Deleted_at IS NULL";
                    command.Parameters.AddWithValue("@idUser", transacao.payment_reference);
                    reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        return BadRequest("This User doesn't exist");
                    }

                    command.CommandText = "Update Users Set Balance = @balance where Id = @idUser";
                    command.Parameters.AddWithValue("@Balance", (float)(double)(Convert.ToInt32(reader["Balance"]) + transacao.montante));
                    reader.Close();
                    command.ExecuteNonQuery();
                    
                }
                command.CommandText = "Insert INTO Transacoes(Id_vcard,TipoTransacao,Montante,Data,Id_category,Phone_transaction,TipoPayment,Payment_reference) values(@id_vcard,@tipoTransacao,@montante,@date,@id_category,@phone_transaction,@tipoPayment,@payment_reference) ";
                command.Parameters.AddWithValue("@tipoTransacao", transacao.tipoTransacao);
                command.Parameters.AddWithValue("@id_vcard", transacao.id_vcard);
                command.Parameters.AddWithValue("@montante", transacao.montante);
                command.Parameters.AddWithValue("@phone_transaction", transacao.phone_transaction);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.Parameters.AddWithValue("@id_category", transacao.id_category);
                command.Parameters.AddWithValue("@tipoPayment", transacao.tipopayment);
                command.Parameters.AddWithValue("@payment_reference", transacao.payment_reference);
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
                return Ok(transacao);
            }
            catch (Exception e)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest();
            }
        }

        //this method create a transaction if the transaction on the second bank have Error
        [Route("crediterror")]
        [HttpPost]
        public IHttpActionResult PostTransacaoCreditOnError(Transacao transacao)
        {
            HttpContext httpContext = HttpContext.Current;
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            SqlDataReader reader;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;
            if (!transacao.checkAccessToken())
            {
                return BadRequest("You dont have permission for this operation");
            }
            try
            {
                command.CommandText = "Select * from Vcards where Phone_number=@phone_number AND Deleted_at IS NULL";
                command.Parameters.AddWithValue("@phone_number", transacao.id_vcard);
                reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return BadRequest("This Vcard doesnt exist");
                }
                command.CommandText = "Update Vcards Set Balance = @balance where Phone_number = @phone_number";
                command.Parameters.AddWithValue("@Balance", (float)(double)(Convert.ToInt32(reader["Balance"]) + transacao.montante));
                reader.Close();
                command.ExecuteNonQuery();
                transacao.payment_reference = transacao.phone_transaction;


                command.CommandText = "Insert INTO Transacoes(Id_vcard,TipoTransacao,Montante,Data,Id_category,Phone_transaction,TipoPayment,Payment_reference) values(@id_vcard,@tipoTransacao,@montante,@date,@id_category,@phone_transaction,@tipoPayment,@payment_reference) ";
                command.Parameters.AddWithValue("@tipoTransacao", transacao.tipoTransacao);
                command.Parameters.AddWithValue("@id_vcard", transacao.id_vcard);
                command.Parameters.AddWithValue("@montante", transacao.montante);
                command.Parameters.AddWithValue("@phone_transaction", transacao.phone_transaction);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.Parameters.AddWithValue("@id_category", transacao.id_category);
                command.Parameters.AddWithValue("@tipoPayment", transacao.tipopayment);
                command.Parameters.AddWithValue("@payment_reference", transacao.payment_reference);
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
                return Ok();
            }
            catch (Exception e)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest();
            }
        }

        //This method isnt used
        [Route("{id:int}")]
        public IHttpActionResult DeleteTransacao(int id)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction; ;
            try
            {
                command.CommandText = "Update Transacoes Set Deleted_at=@date Where Id=@id ";
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.ExecuteNonQuery();
                conn.Close();
                return Ok();
            }
            catch
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
                return NotFound();
            }
        }
    }
}