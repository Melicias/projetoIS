using vcardAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;

namespace vcardAPI.Controllers
{
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        // GET: Admins
        //[AllowAnonymous]
        [Route("")]
        [Authorize]
        public IEnumerable<Admin> GetAllAdmins()
        {
            List<Admin> admins = new List<Admin>();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                var identity = (ClaimsIdentity)User.Identity;
                int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
                SqlCommand command = new SqlCommand("Select * From Admin where Id != @id Order By Id", conn);
                command.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Admin admin = new Admin
                    {
                        id = (int)reader["Id"],
                        nome = (string)reader["Nome"],
                        email = (string)reader["Email"],
                        password = "",
                        is_active = (int)Convert.ToInt16(reader["Is_active"])
                    };
                    admins.Add(admin);
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
            return admins;
        }


        [Route("authenticate")]
        [HttpGet]
        [Authorize]
        public Admin GetAdmin()
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                var identity = (ClaimsIdentity)User.Identity;
                int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
                SqlCommand command = new SqlCommand("Select * From Admin where Id = @id", conn);
                command.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = command.ExecuteReader();
                Admin admin;
                if (reader.Read())
                {
                    admin = new Admin
                    {
                        id = (int)reader["Id"],
                        nome = (string)reader["Nome"],
                        email = (string)reader["Email"],
                        password = "",
                        is_active = (int)Convert.ToInt16(reader["Is_active"])
                    };
                    conn.Close();
                    return admin;
                }
                conn.Close();
                return null;
            }
            catch (Exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return null;
            }
        }

        [Route("{idAdmin}")]
        [Authorize]
        public IHttpActionResult Patch(int idAdmin, [FromBody] int is_active)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Update Admin set Is_active = @isActive where Id = @id", conn);
                command.Parameters.AddWithValue("@id", idAdmin);
                command.Parameters.AddWithValue("@isActive", is_active);

                int numRegistos = command.ExecuteNonQuery();
                conn.Close();
                if (numRegistos > 0)
                {
                    return Ok(); // 200 OK
                }
                return BadRequest("Nothing was updated"); // Bad Request
            }
            catch (Exception)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest("Something whent wrong with the update");
            }
        }

        [Route("")]
        [Authorize]
        public IHttpActionResult PostAdmin(Admin admin)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Insert INTO Admin (Nome, Email, Password) values(@Nome,@Email,@Password) ", conn);
                command.Parameters.AddWithValue("@Nome", admin.nome);
                command.Parameters.AddWithValue("@Email", admin.email);
                var hash = SecurePasswordHasher.Hash(admin.password);
                command.Parameters.AddWithValue("@Password", hash);
                int number = command.ExecuteNonQuery();
                if(number > 0)
                {
                    command = new SqlCommand("Select * From Admin where Email = @email Order By Id", conn);
                    command.Parameters.AddWithValue("@email", admin.email);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        admin = new Admin
                        {
                            id = (int)reader["Id"],
                            nome = (string)reader["Nome"],
                            email = (string)reader["Email"],
                            password = "",
                            is_active = (int)Convert.ToInt16(reader["Is_active"])
                        };
                        conn.Close();
                        return Ok(admin);
                    }
                }
                conn.Close();
                return BadRequest("No admin found");
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest("Something went wrong");
            }
        }

        [Route("{idAdmin}")]
        [Authorize]
        public IHttpActionResult DeleteAdmin(int idAdmin)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Delete from Admin where Id = @id", conn);
                command.Parameters.AddWithValue("@id", idAdmin);

                int numRegistos = command.ExecuteNonQuery();
                conn.Close();
                if (numRegistos > 0)
                {
                    return Ok(); // 200 OK
                }
                return BadRequest("Nothing done"); // Bad Request
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return BadRequest("Something went wrong");
            }
        }

        [Route("changePassword")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult PostChangePassword(Password pass)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                var identity = (ClaimsIdentity)User.Identity;
                int id = Int32.Parse(identity.Claims.Where(c => c.Type == "id").Single().Value);
                SqlCommand command = new SqlCommand("Select * From Admin where Id = @id", conn);
                command.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = command.ExecuteReader();
                Admin admin;
                if (reader.Read())
                {
                    admin = new Admin
                    {
                        id = (int)reader["Id"],
                        nome = (string)reader["Nome"],
                        email = (string)reader["Email"],
                        password = (string)reader["Password"],
                        is_active = (int)Convert.ToInt16(reader["Is_active"])
                    };
                    reader.Close();
                    if (SecurePasswordHasher.Verify(pass.antiga, admin.password))
                    {
                        command = new SqlCommand("Update Admin set password = @password where Id = @id", conn);
                        var hash = SecurePasswordHasher.Hash(pass.nova);
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@password", hash);

                        int numRegistos = command.ExecuteNonQuery();
                        conn.Close();
                        if (numRegistos > 0)
                        {
                            return Ok(); // 200 OK
                        }
                        return BadRequest("Password not changed"); // Bad Request
                    }
                }
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                throw;
            }
            return BadRequest("Something went wrong");
        }


    }
}