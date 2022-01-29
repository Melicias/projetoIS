using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Banco1.Models;
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace Banco1
{
    public class APIAUTHORIZATIONSERVERPROVIDER : OAuthAuthorizationServerProvider
    {
        int idUser = -1;
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated(); //   
        }

        public override Task TokenEndpointResponse(OAuthTokenEndpointResponseContext context)
        {
            string thisIsTheToken = context.AccessToken;
            //add user Id and status as additional response parameter
            if(idUser != -1)
                context.AdditionalResponseParameters.Add("id", idUser+"");
            return base.TokenEndpointResponse(context);
        }
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            User user = getUserByEmail(context.UserName);
            if(user != null)
            {
                if (SecurePasswordHasher.Verify(context.Password, user.password))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
                    idUser = user.id;
                    identity.AddClaim(new Claim("id", user.id+""));
                    identity.AddClaim(new Claim(ClaimTypes.Name, user.email));
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
                context.SetError("User not found");
                return;
            }
        }

        public User getUserByEmail(String email)
        {
            string connectionString = Properties.Settings.Default.ConnStr;
            SqlConnection conn = null;
            User user = null;
            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("Select * From Users where Email = @Email AND Deleted_at IS NULL", conn);
                command.Parameters.AddWithValue("@Email", email);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new User
                    {
                        id = (int)reader["Id"],
                        name = (string)reader["Name"],
                        email = (string)reader["Email"],
                        password = (string)reader["Password"]
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
            return user;
        }
    }
}