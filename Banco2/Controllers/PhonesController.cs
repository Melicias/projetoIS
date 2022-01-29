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
    [RoutePrefix("api/phone")]
    public class PhonesController : ApiController
    {
        // GET: Phones
        //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["BancoApp.Properties.Settings.ConnectionToDB"].ConnectionString;
        string connectionString = Properties.Settings.Default.ConnStr;
        // GET: Users
        //This method isnt used
        [Route("")]
        public IEnumerable<Phone> GetAllPhones()
        {
            List<Phone> phones = new List<Phone>();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Phones Order By Phone_number AND IsNUll(Deleted_at) ", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                   Phone phone = new Phone
                    {
                        id_user = (int)reader["Id_user"],
                        phone_number = (string)reader["Phone_number"],
                    };
                    phones.Add(phone);
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
            return phones;
        }


        [Route("{phone_number}")] //specifies that the id parameter is an integer
        public IHttpActionResult GetPhone(string phone_number)
        {
            Phone phone = new Phone();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Phones Where Phone_number = @phone_number AND Deleted_at Is Null  ", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    phone.id_user = (int)reader["Id_user"];
                    phone.phone_number = (string)reader["Phone_number"];
                }
                reader.Close();
                conn.Close();
                return Ok(phone);

            }
            catch(Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }

        //This method isnt used
        [Route("{phone_number}/user")]
        public IHttpActionResult GetuserByPhone(string phone_number)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Users u Join Phones p On (u.id=p.id_user) Where p.Phone_number = @phone_number AND u.Deleted_at Is Null ", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                SqlDataReader reader = command.ExecuteReader();
                    User user = new User
                    {
                        id = (int)reader["Id"],
                        name = (string)reader["Name"],
                        password = (string)reader["Password"],
                        email = (string)(reader["Email"])
                    };
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
        public IHttpActionResult PostPhone(Phone p)
        {
            if(!p.phone_number.StartsWith("9") || p.phone_number.Length != 9)
            {
                return BadRequest("This phone does't follow PT phone rules");
            }

            var identity = (ClaimsIdentity)User.Identity;
            int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                command.CommandText = "Insert INTO Phones(Id_user,Phone_number) values(@id_user,@phone_number) ";
                command.Parameters.AddWithValue("@id_user", id);
                command.Parameters.AddWithValue("@phone_number", p.phone_number);
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();

                return Ok("Phone added with success");

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
                    Console.WriteLine("Message: {0}", ex2.Message);
                }
                return BadRequest("This phone number already exists");
            }
        }

        //This method isnt used
        [Route("{phone_number}")]
        public IHttpActionResult PutPhone(string phone_number, Phone p)
        {

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                if (p.phone_number.StartsWith("9")){
                    command.CommandText = "UPDATE Phones SET Id_user=@id_user Where Phone_number=@phone_number ";
                    command.Parameters.AddWithValue("@id_user", p.id_user);
                    command.Parameters.AddWithValue("@phone_number", phone_number);
                    int nrows = command.ExecuteNonQuery();
                    conn.Close();
                    if (nrows > 0)
                        return Ok(p);
                    else
                        return NotFound();
                }
                else
                {
                    throw new Exception();
                }
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
        public IHttpActionResult DeletePhone(string phone_number)
        {
            SqlConnection conn = null;
            var identity = (ClaimsIdentity)User.Identity;
            int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * from Vcards where Phone_number=@phone_number AND Deleted_at is NULL", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                int nrows = command.ExecuteNonQuery();
                if (nrows > 0)
                {
                    conn.Close();
                    return BadRequest("Phone number was a vcard associated");
                }
                command = new SqlCommand("Select * from Phones where Phone_number=@phone_number AND Deleted_at is NULL", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                nrows = command.ExecuteNonQuery();
                if (nrows == -1)
                {
                    conn.Close();
                    return BadRequest("This phone number doesnt exist");
                }
                command = new SqlCommand("Update Phones Set Deleted_at=@date Where Phone_number=@phone_number ", conn);
                command.Parameters.AddWithValue("@phone_number", phone_number);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.ExecuteNonQuery();
                conn.Close();
                return Ok("Deleted with success");
            }
            catch(Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }
    }
}