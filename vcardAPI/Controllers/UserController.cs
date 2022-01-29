using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using vcardAPI.Models;

namespace vcardAPI.Controllers
{
    public class Login{
        public string email { get; set; }
        public string password { get; set; }
    }

    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        // GET: User
        string connectionString = Properties.Settings.Default.ConnStr;

        [Route("login")]
        [HttpPost]
        public IHttpActionResult Login(Login login)
        {
            try
            {
                SqlConnection conn = null;
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Users where Email = @email Order By Id", conn);
                command.Parameters.AddWithValue("@email", login.email);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int idBanco = (int)reader["Id_banco"];
                    int idUser = (int)reader["Id"];
                    reader.Close();
                    command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                    command.Parameters.AddWithValue("@id", idBanco);
                    reader = command.ExecuteReader();
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
                                var request = new RestSharp.RestRequest("token", RestSharp.Method.POST);
                                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                                request.AddParameter("application/x-www-form-urlencoded",
                                    $"grant_type=password&username={login.email}&password={login.password}", ParameterType.RequestBody);
                                request.AddHeader("Accept", "application/json");
                                RestSharp.IRestResponse response = client.Execute(request);
                                String content = response.Content;
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    JsonDeserializer deserial = new JsonDeserializer();
                                    User us = deserial.Deserialize<User>(response);
                                    us.email = login.email;
                                    us.idBanco = idBanco;
                                    us.uniqueId = Convert.ToInt32(idUser);
                                    try { 
                                        MqttClient m_cClient = new MqttClient("127.0.0.1");
                                        m_cClient.Connect(Guid.NewGuid().ToString());
                                        byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
                                        string[] m_strTopicsInfo = {(idUser+"")};
                                        m_cClient.Subscribe(m_strTopicsInfo, qosLevels);
                                    }catch(Exception e)
                                    {

                                    }
                                    return Ok(us);
                                }
                                return BadRequest(content);
                            }
                            catch (System.Net.WebException ex)
                            {
                                //do something here to make the site unusable, e.g:
                                return NotFound();
                            }
                        }
                    }
                }
                else
                {
                    return BadRequest("Email not found");
                }
            }
            catch (Exception ee)
            {
                return BadRequest("Email not sent");
            }
            return BadRequest();
        }

        [Route("newnumber/{idBanco}")]
        [HttpPost]
        public IHttpActionResult postNovonr(int idBanco,Phone phone)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = conn = new SqlConnection(connectionString);
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
                            var request = new RestSharp.RestRequest("api/phone", RestSharp.Method.POST);
                            request.AddHeader("Authorization", authHeader);
                            request.AddHeader("Accept", "application/json");
                            request.AddJsonBody(phone);
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                return Ok(content);
                            }
                            return BadRequest(content);
                        }
                        catch (System.Net.WebException ex)
                        {
                            return BadRequest("There is some problem with this bank url");
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                return BadRequest("Bank url with problems, or maybe wrong");
            }
           
            return BadRequest();
        }



        [Route("phones/{idBanco}")]
        [HttpGet]
        public IHttpActionResult getNrtelemoveis(int idBanco)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", idBanco);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String ip = @"" + (string)reader["Ip"] + "";
                    if (ip != null)
                    {
                        reader.Close();
                        conn.Close();
                        var client = new RestSharp.RestClient(ip);
                        var request = new RestSharp.RestRequest("api/users/phone", RestSharp.Method.GET);
                        request.AddHeader("Authorization", authHeader);
                        request.AddHeader("Accept", "application/json");
                        RestSharp.IRestResponse response = client.Execute(request);
                        String content = response.Content;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            List<Phone> phones = client.Execute<List<Phone>>(request).Data;
                            return Ok(phones);
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

        [Route("phone/{idBanco}")]
        [HttpDelete]
        public IHttpActionResult deleteNrtelemovel(int idBanco, Phone phone)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                if(phone.phone_number == null)
                {
                    BadRequest("Need phone number to delete");
                }
                SqlCommand command = new SqlCommand("Select * From Vcards where Phone_number = @phone_number", conn);
                command.Parameters.AddWithValue("@phone_number", phone.phone_number);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    reader.Close();
                    conn.Close();
                    BadRequest("This phone number has a Vcard");
                }
                reader.Close();
                command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", idBanco);
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String ip = @"" + (string)reader["Ip"] + "";
                    if (ip != null)
                    {
                        try
                        {
                            reader.Close();
                            var client = new RestSharp.RestClient(ip);
                            var request = new RestSharp.RestRequest("api/phone/" + phone.phone_number + "", RestSharp.Method.DELETE);
                            request.AddHeader("Authorization", authHeader);
                            request.AddHeader("Accept", "application/json");
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                conn.Close();
                                return Ok(content);
                            }
                            conn.Close();
                            return BadRequest(content);
                        }
                        catch (System.Net.WebException ex)
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                return BadRequest();
            }
            return BadRequest();
        }
        
        [Route("{idBanco}")]
        [HttpGet]
        public IHttpActionResult getUser(int idBanco)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select Ip from Bancos where id = @idBanco", conn);
                command.Parameters.AddWithValue("@idBanco", idBanco);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String ip = @"" + (string)reader["Ip"] + "";
                    if (ip != null)
                    {
                        try
                        {
                            reader.Close();
                            var client = new RestSharp.RestClient(ip);
                            var request = new RestSharp.RestRequest("api/users/me", RestSharp.Method.GET);
                            request.AddHeader("Authorization", authHeader);
                            request.AddHeader("Accept", "application/json");
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                JsonDeserializer deserial = new JsonDeserializer();
                                User user = deserial.Deserialize<User>(response);
                                user.idBanco = idBanco;

                                command = new SqlCommand("Select * From Users where Id_banco = @idBanco and Id_user = @idUser Order By Id", conn);
                                command.Parameters.AddWithValue("@idBanco", idBanco);
                                command.Parameters.AddWithValue("@idUser", user.id);
                                reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    user.uniqueId = (int)reader["Id"];
                                }
                                conn.Close();
                                return Ok(user);
                            }
                            conn.Close();
                            return BadRequest(content);
                        }
                        catch (System.Net.WebException ex)
                        {
                            //do something here to make the site unusable, e.g:
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                return BadRequest("Phone number not found in vcards");
            }
            return BadRequest();
        }
    }
}