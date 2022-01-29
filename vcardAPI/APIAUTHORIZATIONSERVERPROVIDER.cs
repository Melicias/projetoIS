using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using vcardAPI.Models;

namespace vcardAPI
{
    public class APIAUTHORIZATIONSERVERPROVIDER : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated(); //   
        }
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            Admin admin = getAdminByEmail(context.UserName);
            if(admin != null)
            {
                if(admin.is_active == 1)
                {
                    if (SecurePasswordHasher.Verify(context.Password, admin.password))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                        identity.AddClaim(new Claim("id", admin.id+""));
                        identity.AddClaim(new Claim(ClaimTypes.Name, admin.email));
                        context.Validated(identity);
                    }
                    else
                    {
                        context.SetError("Provided password is incorrect");
                        return;
                    }
                }
                else
                {
                    context.SetError("Admin is blocked");
                    return;
                }
            }
            else
            {
                context.SetError("Admin not found");
                return;
            }
        }

        public Admin getAdminByEmail(String email)
        {
            string connectionString = Properties.Settings.Default.ConnStr;
            SqlConnection conn = null;
            Admin admin = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Admin where Email = @Email", conn);
                command.Parameters.AddWithValue("@Email", email);

                SqlDataReader reader = command.ExecuteReader();
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
            return admin;
        }
    }
}