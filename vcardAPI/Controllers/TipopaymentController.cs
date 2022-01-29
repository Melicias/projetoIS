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
    [RoutePrefix("api/tipopayment")]
    public class TipopaymentController : ApiController
    {
        string connectionString = Properties.Settings.Default.ConnStr;

        [Route("")]
        public IEnumerable<Tipopayment> GetAllTipopayment()
        {
            List<Tipopayment> tipopayments = new List<Tipopayment>();
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From TipoPayment Order By Id", conn);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Tipopayment tipopayment = new Tipopayment
                    {
                        id = (int)reader["Id"],
                        code = (string)reader["Code"],
                        name = (string)reader["Name"],
                        deleted_at = (DBNull.Value.Equals(reader["deleted_at"]) ? false : true),
                    };
                    tipopayments.Add(tipopayment);
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception e)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return null;
            }
            return tipopayments;
        }

        [Route("")]
        [Authorize]
        public IHttpActionResult PostTipopayment(Tipopayment tipopayment)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Insert INTO TipoPayment (Code,Name) values(@code,@name); SELECT SCOPE_IDENTITY();", conn);
                command.Parameters.AddWithValue("@name", tipopayment.name);
                command.Parameters.AddWithValue("@code", tipopayment.code.ToUpper());

                var id = command.ExecuteScalar();
                if (id != null)
                {
                    tipopayment.code = tipopayment.code.ToUpper();
                    tipopayment.id = (int)Convert.ToInt16(id);
                    tipopayment.deleted_at = false;
                    conn.Close();
                    return Ok(tipopayment);
                }
                conn.Close();
                return BadRequest();
            }
            catch (SqlException exception)
            {
                if(exception.Number == 2627)
                {
                    string msg = "Code already in use!";
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

        [Route("{idPaymenttype}")]
        [Authorize]
        public IHttpActionResult PutTipopayment(int idPaymenttype, Tipopayment tipopayment)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command;
                command = new SqlCommand("update TipoPayment set name = @name where id = @id", conn);
                command.Parameters.AddWithValue("@name", tipopayment.name);
                command.Parameters.AddWithValue("@id", idPaymenttype);

                var result = command.ExecuteNonQuery();
                if (result > 0)
                {
                    conn.Close();
                    return Ok(tipopayment);
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