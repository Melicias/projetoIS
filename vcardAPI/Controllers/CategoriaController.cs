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
    [RoutePrefix("api/categoria")]
    public class CategoriaController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        [Route("")]
        public IEnumerable<Categorias> GetAllCategorias()
        {
            List<Categorias> categorias = new List<Categorias>();
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Categorias Order By Id", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Categorias categoria = new Categorias
                    {
                        id = (int)reader["Id"],
                        nome = (string)reader["Nome"],
                        deleted_at = (DBNull.Value.Equals(reader["deleted_at"]) ? false : true),
                    };
                    categorias.Add(categoria);
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
            return categorias;
        }

        [Route("")]
        [Authorize]
        public IHttpActionResult PostCategoria(Categorias categoria)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Insert INTO Categorias (Nome) values(@nome); SELECT SCOPE_IDENTITY();", conn);
                command.Parameters.AddWithValue("@nome", categoria.nome);

                var id = command.ExecuteScalar();
                if (id != null)
                {
                    categoria.id = (int)Convert.ToInt16(id);
                    categoria.deleted_at = false;
                    conn.Close();
                    return Ok(categoria);
                }
                conn.Close();
                return BadRequest();
            }
            catch (SqlException exception)
            {
                if(exception.Number == 2627)
                {
                    string msg = "Name already in use!";
                    return Content((HttpStatusCode)422, new Error {error=msg});
                }
                return NotFound();
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return NotFound();
            }
        }

        [Route("{idCategory}")]
        [Authorize]
        public IHttpActionResult PutCategoria(int idCategory, Categorias categoria)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command;
                if (categoria.deleted_at)
                {
                    command = new SqlCommand("update Categorias set deleted_at = GETDATE(), Nome = @nome where id = @id", conn);
                }
                else
                {
                    command = new SqlCommand("update Categorias set deleted_at = NULL, Nome = @nome where id = @id", conn);
                }
                command.Parameters.AddWithValue("@nome", categoria.nome);
                command.Parameters.AddWithValue("@id", idCategory);

                var result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    conn.Close();
                    return Ok(categoria);
                }
                conn.Close();
                return BadRequest();
            }
            catch (SqlException exception)
            {
                if (exception.Number == 2627)
                {
                    string msg = "Name already in use!";
                    return Content((HttpStatusCode)422, new Error { error = msg });
                }
                return NotFound();
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