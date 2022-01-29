using Banco1.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Banco1.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["BancoApp.Properties.Settings.ConnectionToDB"].ConnectionString;
        string connectionString = Properties.Settings.Default.ConnStr;

        // GET: Users
        [Route("")]
        public IEnumerable<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Users Order By Id", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User user = new User
                    {
                        id = (int)reader["Id"],
                        name = (string)reader["Name"],
                        email = (string)reader["email"],
                        password = (string)reader["Password"],
                    };
                   users.Add(user);
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
            return users;
        }

        //This method isnt used
        [Route("{id}")]
        public IHttpActionResult GetUsers(int id)
        {
            User user = new User();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Users Where Id = @iduser AND Deleted_at IS NULL", conn);
                command.Parameters.AddWithValue("@iduser", id);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user.id = (int)reader["Id"];
                    user.name = (string)reader["Name"];
                    user.email = (string)reader["Password"];
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

        [Route("me")]
        [Authorize]
        public IHttpActionResult GetUser()
        {
            User user = new User();
            SqlConnection conn = null;
            var identity = (ClaimsIdentity)User.Identity;
            int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Users Where Id = @iduser AND Deleted_at IS NULL", conn);
                command.Parameters.AddWithValue("@iduser", id);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user.id = (int)reader["Id"];
                    user.balance = (double)reader["Balance"];
                    user.name = (string)reader["Name"];
                    user.email = (string)reader["email"];
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

        [Route("phone")] //specifies that the id parameter is an integer
        [Authorize]
        public IHttpActionResult GetPhoneFromUser()
        {
            List<Phone> phones = new List<Phone>();
            SqlConnection conn = null;

            var identity = (ClaimsIdentity)User.Identity;
            int id_user = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select p.*, ISNULL(v.Id ,0) as vcard From Phones p left join Vcards v on p.Phone_number = v.Phone_number where p.Id_user = @iduser AND  p.Deleted_at IS NULL ", conn);
                command.Parameters.AddWithValue("@iduser", id_user);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Phone p = new Phone
                    {
                        phone_number = (string)reader["Phone_number"],
                        id_user = (int)reader["Id_user"],
                        vcard = (int)reader["vcard"]
                    };
                    phones.Add(p);
                }
            
                reader.Close();
                conn.Close();
                return Ok(phones);

            }
            catch
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest("Some error trying to find the user or phone");
            }
        }

        [Route("")]
        [HttpPost]
        public IHttpActionResult PostUser(UserPhone userPhone)
        {
            if((!userPhone.phone.phone_number.StartsWith("9") && userPhone.phone.phone_number.Length != 9)){
                return BadRequest("Number with PT thing");
            }
            
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;

            try
            {
                command.CommandText = "Insert INTO Users(Name,Email,Password) OUTPUT Inserted.Id values(@name,@email,@password) ";
                command.Parameters.AddWithValue("@name", userPhone.user.name);
                command.Parameters.AddWithValue("@email", userPhone.user.email);
                command.Parameters.AddWithValue("@password", SecurePasswordHasher.Hash(userPhone.user.password));
                SqlDataReader reader = command.ExecuteReader();
                
                if (reader.Read())
                {
                    userPhone.phone.id_user = (int)reader["Id"];
                    userPhone.user.id = userPhone.phone.id_user;
                }
                
                reader.Close();
                conn = new SqlConnection(connectionString);
                conn.Open();
                command.CommandText = "Insert INTO Phones(Id_user,Phone_number) values(@id_user,@phone_number) ";
                command.Parameters.AddWithValue("@id_user", userPhone.phone.id_user);
                command.Parameters.AddWithValue("@phone_number", userPhone.phone.phone_number);
                var result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    userPhone.user.id = userPhone.phone.id_user;
                    transaction.Commit();
                    conn.Close();
                    userPhone.user.password = "";
                    return Ok(userPhone.user);
                }

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

        //This method isnt used
        [Route("{id:int}")]
        public IHttpActionResult PutUser(int id, User user)
        {

            SqlConnection conn = null;
            conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand command = conn.CreateCommand();
            SqlTransaction transaction;
            transaction = conn.BeginTransaction("SampleTransaction");
            command.Transaction = transaction;

            try
            {

                command.CommandText = "UPDATE Users SET Name=@name, Email=@email,Password=@password Where Id=@id AND deleted_at IS NULL ";
                command.Parameters.AddWithValue("@name", user.name);
                command.Parameters.AddWithValue("@email", user.email);
                command.Parameters.AddWithValue("@password", user.password);
                command.Parameters.AddWithValue("@id", id);
                int nrows = command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
                if (nrows > 0)
                    return Ok(user);
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
                                                                        
                                                                                                                                   
        [Route("{id:int}")]                                             
        public IHttpActionResult DeleteUser(int id)                     
        {                                                               
            SqlConnection conn = null;                                  
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Update Users Set Deleted_at=@date Where Id=@id ", conn);

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
                return NotFound();
            }
        }

        [Route("testconnection")]
        [HttpGet]
        public IHttpActionResult testConnection()
        {
            return Ok();
        }
    }
}