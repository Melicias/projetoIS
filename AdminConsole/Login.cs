using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdminConsole.Models;
using RestSharp.Serialization.Json;

namespace AdminConsole
{
    public partial class Login : Form
    {
        string baseURI = @"http://localhost:52388/";
        public Login()
        {
            InitializeComponent();
        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            lbErros.Text = "";
            if (checkValues(tbEmail.Text, tbPassword.Text))
            {
                try
                {
                    var client = new RestSharp.RestClient(baseURI);
                    var request = new RestSharp.RestRequest("token", RestSharp.Method.POST);

                    request.AddHeader("content-type", "application/x-www-form-urlencoded");
                    request.AddParameter("application/x-www-form-urlencoded", 
                        $"grant_type=password&username={this.tbEmail.Text}&password={this.tbPassword.Text}", ParameterType.RequestBody);

                    RestSharp.IRestResponse response = client.Execute(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        JsonDeserializer deserial = new JsonDeserializer();
                        Admin ad = deserial.Deserialize<Admin>(response);

                        client = new RestSharp.RestClient(baseURI);
                        request = new RestSharp.RestRequest("api/admin/authenticate", RestSharp.Method.GET);
                        request.AddHeader("Accept", "application/json");
                        request.AddHeader("Authorization", "Bearer " + ad.accessToken);

                        Admin admin = client.Execute<Admin>(request).Data;
                        admin.accessToken = ad.accessToken;

                        if(admin.email != null)
                        {
                            Menu menu = new Menu(admin);
                            menu.Show();
                            this.Hide();
                        }
                    }
                    else
                    {
                        JsonDeserializer deserial = new JsonDeserializer();
                        Error error = deserial.Deserialize<Error>(response);
                        lbErros.Text = error.error;
                    }
                }
                catch(Exception ee)
                {
                    lbErros.Text = "Something went wrong";
                }
            }
        }

        bool checkValues(string email, string password)
        {
            if(email.Length > 0 || password.Length > 0)
            {
                if (isValidEmail(email))
                {
                    return true;
                }
            }
            else
            {
                lbErros.Text = "The fields can't be empty!";
            }
            return false;
        }

        bool isValidEmail(string email)
        {
            bool result = false;
            try
            {
                var eMailValidator = new System.Net.Mail.MailAddress(email);
                result = (email.LastIndexOf(".") > email.LastIndexOf("@"));
            }
            catch
            {
                result = false;
            };
            return result;
        }
    }
}
