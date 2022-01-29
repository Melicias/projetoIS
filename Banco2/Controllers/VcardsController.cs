using Banco1.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace Banco1.Controllers
{
    [RoutePrefix("api/vcard")]
    public class VcardsController : ApiController
    {
        // GET: Vcards
        //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["BancoApp.Properties.Settings.ConnectionToDB"].ConnectionString;
        string connectionString = Properties.Settings.Default.ConnStr;

        //This method isnt used
        [Route("")]
        public IEnumerable<Vcard> GetAllVcards()
        {
            List<Vcard> vcards = new List<Vcard>();
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Vcards Where  Deleted_at IS NULL Order By Id", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Vcard vcard = new Vcard
                    {
                        id = (int)reader["Id"],
                        phone_number = (string)reader["Phone_number"],
                        balance = (double)reader["Balance"],
                        code = (string)reader["Code"]
                    };
                    vcards.Add(vcard);
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                throw;
            }
            return vcards;
        }

        //This method isnt used
        [Route("{id:int}/user")] //specifies that the id parameter is an integer                  
        public IHttpActionResult GetUserFromVcard(int id)
        {
            User user = new User();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select u.* From Users u Join Phones p On (u.id=p.id_user) Join  Vcards v On (p.phone_number = v.phone_number) Where Id = @id AND  Deleted_at IS NULL", conn);
                command.Parameters.AddWithValue("@id", id);


                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user.id = (int)reader["Id"];
                    user.name = (string)reader["Name"];
                    user.email = (string)reader["Email"];
                    user.password = (string)reader["Password"];


                }
                reader.Close();
                conn.Close();
                return Ok(user);

            }
            catch
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }

        }


        [Route("")]
        [Authorize]
        public IHttpActionResult PostVcard(Vcard vcard)
        {
            var identity = (ClaimsIdentity)User.Identity;
            int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);

            if (vcard.code == null || vcard.phone_number == null)
            {
                return BadRequest("The request must have a code and a phone number");
            }
            if (vcard.code.Length != 4)
            {
                return BadRequest("The code must be 4 numbers long");
            }

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;
            try
            {
                command.CommandText = "Select * From Phones  Where Phone_number=@phone_number";
                command.Parameters.AddWithValue("@phone_number", vcard.phone_number);
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return BadRequest("Phone number not found");
                }
                if ((int)reader["Id_user"] != id)
                {
                    return BadRequest("The number does not belong to this user");
                }
                reader.Close();
                command.CommandText = "Select * From Vcards Where Phone_number=@phone_number AND Deleted_at is Null";
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    reader.Close();
                    command.CommandText = "Update Vcards Set Deleted_at=@Deleted_at , Balance=@balanceDeleted where Phone_number=@phone_number";
                    command.Parameters.AddWithValue("@BalanceDeleted", 0.0);
                    command.Parameters.AddWithValue("@Deleted_at", null);
                    command.ExecuteNonQuery();
                }
                reader.Close();
                command.CommandText = "Insert INTO Vcards(Phone_number,Code) values(@Phone_number,@Code)";
                command.Parameters.AddWithValue("@Balance", 0.0);
                command.Parameters.AddWithValue("@Code", SecurePasswordHasher.Hash(vcard.code));
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
                return Ok("Vcard created with success");
            }
            catch (Exception e)
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


        [Route("{id:int}")]
        //This method isnt used
        public IHttpActionResult PutVcard(int id, Vcard vcard)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;
            try
            {
                command.CommandText = "UPDATE Vcards SET Code=@code Where id=@id AND Deleted_at IS NULL ";
                command.Parameters.AddWithValue("@code", vcard.code);
                command.Parameters.AddWithValue("@id", id);
                int nrows = command.ExecuteNonQuery();
                conn.Close();
                if (nrows > 0)
                    return Ok(vcard);
                else
                    return NotFound();
            }
            catch (Exception)
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

        [Route("{phone_number}")]
        [Authorize]
        public IHttpActionResult DeleteVcard(string phone_number, Vcard vcard)
        {
            var identity = (ClaimsIdentity)User.Identity;
            int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select v.*, p.Id_user From Vcards v inner join Phones p on p.Phone_number = v.Phone_number Where v.Phone_number = @phone_number AND v.Deleted_at IS NULL", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return BadRequest("There is no vcard with this number");
                }
                if ((int)reader["Id_user"] != id)
                {
                    return BadRequest("The vcard is not yours");
                }
                if ((double)reader["Balance"] != 0)
                {
                    return BadRequest("The balance should be 0 to delete an vcard");
                }
                if (!SecurePasswordHasher.Verify(vcard.code, (string)reader["Code"]))
                {
                    return BadRequest("The code is not correct");
                }
                reader.Close();
                command = new SqlCommand("Update Vcards Set Deleted_at=@date Where Phone_number=@id", conn);
                command.Parameters.AddWithValue("@id", phone_number);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.ExecuteNonQuery();
                conn.Close();
                return Ok("Vcard deleted with success");
            }
            catch
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();

                return NotFound();
            }
        }

        [Route("{phone_number:int}")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult GetVcardByPhoneNumber(int phone_number)
        {
            var identity = (ClaimsIdentity)User.Identity;
            int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            Vcard vcard = new Vcard();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select v.* From Vcards v inner join Phones p on p.Phone_number = v.Phone_number Where v.Phone_number = @phone_number AND p.Id_user = @id AND v.Deleted_at IS NULL", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                command.Parameters.AddWithValue("@id", id);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    vcard.id = (int)reader["Id"];
                    vcard.phone_number = (string)reader["Phone_number"];
                    vcard.balance = (double)reader["Balance"];
                    vcard.code = null;
                    reader.Close();
                    conn.Close();
                    return Ok(vcard);
                }
                reader.Close();
                conn.Close();
                return BadRequest("Vcard not found");
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }

        [Route("id/{phone_number:int}")]
        [HttpGet]
        public IHttpActionResult GetIdVcardByPhoneNumber(int phone_number)
        {
            var identity = (ClaimsIdentity)User.Identity;
            int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            Vcard vcard = new Vcard();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select .* From Vcards v inner join Phones p on p.Phone_number = v.Phone_number Where v.Phone_number = @phone_number AND v.Deleted_at IS NULL", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    vcard.id = (int)reader["Id"];
                    vcard.phone_number = null;
                    vcard.balance = 0;
                    vcard.code = null;
                    reader.Close();
                    conn.Close();
                    return Ok(vcard);
                }
                reader.Close();
                conn.Close();
                return BadRequest("Vcard not found");
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }
    }
}