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
    [RoutePrefix("api/vcard")]
    public class VcardController : ApiController
    {

        string connectionString = Properties.Settings.Default.ConnStr;
        // GET: Vcard
        [Route("")]
        [HttpPost]
        public IHttpActionResult postVcard(Vcard vcard)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Vcards where Phone_number = @phone_number", conn);
                command.Parameters.AddWithValue("@phone_number", vcard.Phone_number);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    reader.Close();
                    conn.Close();
                    return BadRequest("This phone number is already associated with a vcard");
                }
                reader.Close();
                command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", vcard.id_banco);
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
                            var request = new RestSharp.RestRequest("api/vcard", RestSharp.Method.POST);
                            request.AddHeader("Authorization", authHeader);
                            request.AddHeader("Accept", "application/json");
                            request.AddJsonBody(vcard);
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                command.CommandText = "Insert INTO Vcards (Phone_number,Id_banco) values (@phone_number,@id_banco)";
                                command.Parameters.AddWithValue("@phone_number", vcard.Phone_number);
                                command.Parameters.AddWithValue("@id_banco", vcard.id_banco);
                                command.ExecuteNonQuery();
                                conn.Close();
                                return Ok(content);
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
                return BadRequest();
            }
            return BadRequest();
        }

        [Route("")]
        [HttpDelete]
        public IHttpActionResult DeleteVcard(Vcard vcard)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Bancos where Id = @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", vcard.id_banco);
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
                            var request = new RestSharp.RestRequest("api/vcard/"+ vcard.Phone_number+"", RestSharp.Method.DELETE);
                            request.AddHeader("Authorization", authHeader);
                            request.AddHeader("Accept", "application/json");
                            request.AddJsonBody(vcard);
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                command = new SqlCommand("Delete from Vcards Where Phone_number=@phone_number", conn);
                                command.Parameters.AddWithValue("@phone_number", vcard.Phone_number);
                                command.ExecuteNonQuery();
                                conn.Close();
                                return Ok(content);
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
                return BadRequest();
            }
            return BadRequest();
        }

        [Route("{phone_number}")]
        [HttpGet]
        public IHttpActionResult getVcard(int phone_number)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string authHeader = httpContext.Request.Headers["Authorization"];
                SqlConnection conn = conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select b.Ip From Vcards v left join Bancos b on v.Id_Banco = b.Id where v.Phone_number = @phone_number", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
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
                            var request = new RestSharp.RestRequest("api/vcard/" + phone_number + "", RestSharp.Method.GET);
                            request.AddHeader("Authorization", authHeader);
                            request.AddHeader("Accept", "application/json");
                            RestSharp.IRestResponse response = client.Execute(request);
                            String content = response.Content;

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                JsonDeserializer deserial = new JsonDeserializer();
                                Vcard vcard = deserial.Deserialize<Vcard>(response);
                                conn.Close();
                                return Ok(vcard);
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