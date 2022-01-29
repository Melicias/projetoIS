using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using vcardAPI.Models;

namespace vcardAPI.Controllers
{
    [RoutePrefix("api/banco")]
    public class BancoController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        [Route("")]
        [Authorize]
        public IEnumerable<Banco> GetAllBancos()
        {
            List<Banco> bancos = new List<Banco>();
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Bancos Order By Id", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Banco banco = new Banco
                    {
                        id = (int)reader["Id"],
                        ip = (string)reader["Ip"],
                        name = (string)reader["Name"],
                        isDeleted = (DBNull.Value.Equals(reader["Deleted_at"]) ? false : true),
                        percentagem = (int)Convert.ToInt16(reader["Percentagem"]),
                        max_debit_limit = (int)Convert.ToInt16(reader["Max_debit_limit"])
                    };
                    bancos.Add(banco);
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return null;
            }
            return bancos;
        }

        [Route("getall")]
        [Authorize]
        [HttpGet]
        public IEnumerable<Banco> GetAllBancosNotDeleted()
        {
            List<Banco> bancos = new List<Banco>();
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Bancos where Deleted_at is NULL Order By Id", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Banco banco = new Banco
                    {
                        id = (int)reader["Id"],
                        ip = (string)reader["Ip"],
                        name = (string)reader["Name"],
                        isDeleted = (DBNull.Value.Equals(reader["Deleted_at"]) ? false : true),
                        percentagem = (int)Convert.ToInt16(reader["Percentagem"]),
                        max_debit_limit = (int)Convert.ToInt16(reader["Max_debit_limit"])
                    };
                    bancos.Add(banco);
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return null;
            }
            return bancos;
        }

        [Route("{idBanco}")]
        [Authorize]
        public Banco GetBanco(int idBanco)
        {
            SqlConnection conn = null;
            Banco banco = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", idBanco);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    banco = new Banco
                    {
                        id = (int)reader["Id"],
                        ip = (string)reader["Ip"],
                        name = (string)reader["Name"],
                        isDeleted = (reader["Deleted_at"] == null ? false : true),
                        percentagem = (int)Convert.ToInt16(reader["Percentagem"]),
                        max_debit_limit = (int)Convert.ToInt16(reader["Max_debit_limit"])
                    };
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return null;
            }
            return banco;
        }

        [Route("")]
        [Authorize]
        public IHttpActionResult PostBanco(Banco banco)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Insert INTO Bancos (Ip, Name, Percentagem, Max_debit_limit) values(@ip,@name,@percentagem, @max_debit_limit); SELECT SCOPE_IDENTITY();", conn);
                command.Parameters.AddWithValue("@ip", banco.ip);
                command.Parameters.AddWithValue("@name", banco.name);
                command.Parameters.AddWithValue("@percentagem", banco.percentagem);
                command.Parameters.AddWithValue("@max_debit_limit", banco.max_debit_limit);

                var id = command.ExecuteScalar();
                if (id != null)
                {
                    banco.id = (int)Convert.ToInt16(id);
                    banco.isDeleted = false;
                    conn.Close();
                    return Ok(banco);
                }
                conn.Close();
                return BadRequest("The bank was not created");
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest("Something went wrong");
            }
        }

        [Route("{idBanco}")]
        [Authorize]
        [HttpPut]
        public IHttpActionResult PutBanco(int idBanco, Banco banco)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Update Bancos set Ip = @ip, Name =  @name, Percentagem = @percentagem, Max_debit_limit = @max_debit_limit where id = @id", conn);
                command.Parameters.AddWithValue("@ip", banco.ip);
                command.Parameters.AddWithValue("@name", banco.name);
                command.Parameters.AddWithValue("@percentagem", banco.percentagem);
                command.Parameters.AddWithValue("@max_debit_limit", banco.max_debit_limit);
                command.Parameters.AddWithValue("@id", idBanco);

                var result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    conn.Close();
                    return Ok(banco);
                }
                conn.Close();
                return BadRequest("Not updated");
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest("Somethign went wrong");
            }
        }

        [Route("block/{idBanco}")]
        [Authorize]
        [HttpPatch]
        public IHttpActionResult blockBanco(int idBanco)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("select Deleted_at from Bancos  where id = @id", conn);
                command.Parameters.AddWithValue("@id", idBanco);

                var reader = command.ExecuteReader();
                bool deleted;
                if (reader.Read())
                {
                    if (DBNull.Value.Equals(reader["Deleted_at"]))
                    {
                        command = new SqlCommand("update Bancos set Deleted_at = GETDATE() where id = @id", conn);
                        deleted = true;
                    }
                    else
                    {
                        command = new SqlCommand("update Bancos set Deleted_at = NULL where id = @id", conn);
                        deleted = false;
                    }
                    reader.Close();
                    command.Parameters.AddWithValue("@id", idBanco);
                    var result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        conn.Close();
                        return Ok(deleted);
                    }
                }
                conn.Close();
                return BadRequest("Not blocked");
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest("Somethign went wrong");
            }
        }

        [Route("checkConnection/{idBanco}")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult checkConnection(int idBanco)
        {
            SqlConnection conn = null;
            Banco banco = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", idBanco);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String ip = @""+(string)reader["Ip"]+"";
                    if(ip != null)
                    {
                        try
                        {
                            reader.Close();
                            conn.Close();
                            var client = new RestSharp.RestClient(ip);
                            var request = new RestSharp.RestRequest($"api/users/testconnection", RestSharp.Method.GET);
                            request.AddHeader("Accept", "application/json");

                            RestSharp.IRestResponse response = client.Execute(request);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                return Ok();
                            }
                            else
                            {
                                return NotFound();
                            }   
                        }
                        catch (System.Net.WebException ex)
                        {
                            
                        }
                        return BadRequest("Something went wrong");
                    }
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest("Something went wrong");
            }
            return BadRequest("Bank not found");
        }

        [Route("users/{idBanco}")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult getUsers(int idBanco)
        {
            SqlConnection conn = null;
            Banco banco = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", idBanco);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String ip = @"" + (string)reader["Ip"] + "";
                    if (ip != null)
                    {
                        try
                        {
                            reader.Close();
                            conn.Close();
                            var client = new RestSharp.RestClient(ip);
                            var request = new RestSharp.RestRequest($"api/users", RestSharp.Method.GET);
                            request.AddHeader("Accept", "application/json");
                            

                            RestSharp.IRestResponse result = client.Execute(request);
                            if (result.StatusCode == HttpStatusCode.OK)
                            {
                                JsonDeserializer deserial = new JsonDeserializer();
                                List<User>users = deserial.Deserialize<List<User>>(result);
                                return Ok(users);
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                        catch (System.Net.WebException ex)
                        {
                            return NotFound();
                        }
                        return NotFound();
                    }
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return NotFound();
        }


        [Route("users/{idBanco}")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult postUser(int idBanco, UserPhone userPhone)
        {
            SqlConnection conn = null;
            Banco banco = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Users where Email = @email", conn);
                command.Parameters.AddWithValue("@email", userPhone.user.email);
                var result_phone = command.ExecuteNonQuery();
                if (result_phone == 0)
                {
                    conn.Close();
                    return BadRequest("Email already in use!");
                }
                command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", idBanco);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String ip = @"" + (string)reader["Ip"] + "";
                    if (ip != null)
                    {
                        try
                        {
                            reader.Close();
                            conn.Close();
                            var client = new RestSharp.RestClient(ip);
                            var request = new RestSharp.RestRequest($"api/users", RestSharp.Method.POST);
                            request.AddHeader("Accept", "application/json");
                            request.AddJsonBody(userPhone);

                            RestSharp.IRestResponse result = client.Execute(request);
                            if (result.StatusCode == HttpStatusCode.OK)
                            {
                                JsonDeserializer deserial = new JsonDeserializer();
                                User user = deserial.Deserialize<User>(result);
                                conn = new SqlConnection(connectionString);
                                conn.Open();
                                command = new SqlCommand("Insert INTO Users (Id_banco,Id_user, Email) values(@id,@idUser,@email);", conn);
                                command.Parameters.AddWithValue("@id", idBanco);
                                command.Parameters.AddWithValue("@idUser", user.id);
                                command.Parameters.AddWithValue("@email", userPhone.user.email);

                                var result_vcard = command.ExecuteNonQuery();
                                if (result_vcard > 0)
                                {
                                    return Ok(user);
                                }
                                conn.Close();
                                return BadRequest("Something went wrong and the user was not created"); 
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                        catch (System.Net.WebException ex)
                        {
                            //do something here to make the site unusable, e.g:
                            return NotFound();
                        }
                        return NotFound();
                    }
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                throw;
            }
            return NotFound();
        }

    }
}