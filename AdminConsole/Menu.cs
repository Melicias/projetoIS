using AdminConsole.Models;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AdminConsole
{
    public partial class Menu : Form
    {
        string baseURI = @"http://localhost:52388/";

        MqttClient m_cClient = new MqttClient("127.0.0.1");
        
        private Admin adminLogged = null;
        BindingSource bsAdmins = new BindingSource();
        BindingSource bsBanks = new BindingSource();
        BindingSource bsBanksCb = new BindingSource();
        BindingSource bsCategories = new BindingSource();
        BindingSource bsUsers = new BindingSource();
        BindingSource bsTipopayment = new BindingSource();

        public Menu(Admin admin)
        {
            InitializeComponent();
            this.adminLogged = admin;
            dataGridView1.Columns.Add("id", "Id_user");
            dataGridView1.Columns.Add("msg", "Message");
            DataGridViewColumn column = dataGridView1.Columns[1];
            column.Width = 500;
            try
            {
                m_cClient.Connect(Guid.NewGuid().ToString());
                string[] m_strTopicsInfo = { "#" };
                m_cClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };//QoS
                m_cClient.Subscribe(m_strTopicsInfo, qosLevels);
            }
            catch (Exception e)
            {

            }
            loadBanks();
            loadTipopayment();
            cbCreditDebitTransaction.SelectedIndex = 0;
        }

        private void btLogout_Click(object sender, EventArgs e)
        {
            this.adminLogged = null;
            Form frm = Application.OpenForms["Login"];
            frm.Show();
            this.Close();
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            this.lbEmailAdmin.Text = this.adminLogged.email;
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 0:
                    //configurar bancos
                    loadBanks();
                    loadTipopayment();
                    loadBanksWithUsers();
                    break;
                case 1:
                    //definir default values
                    loadCategories();
                    loadTipopayment();
                    break;
                case 2:
                    //profile
                    loadProfile();

                    break;
                case 3:
                    //manage admins
                    loadManageAdmins();

                    break;
                case 4:
                    loadBanksWithUsers();
                    break;
              
                default:
                    //caso nao seja encontrado

                    break;
            }
        }

       
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {

            //string[] arrParts = strTemp.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);

            //extract XMLNode from XML

            string[] arr = new string[2];
            ListViewItem itm;
            arr[0] = e.Topic;//arrParts[2]; //avatar
            arr[1] = Encoding.UTF8.GetString(e.Message); ;//arrParts[0]; //nickname
            itm = new ListViewItem(arr);

            dataGridView1.BeginInvoke((MethodInvoker)delegate { dataGridView1.Rows.Add(arr[0], arr[1]); });
        }
        private void loadBanksWithUsers()
        {
            //load bank
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest("api/banco/getall", RestSharp.Method.GET);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                List<Banco> bancos = client.Execute<List<Banco>>(request).Data;

                this.bsBanks.DataSource = bancos;
                cbBancoManageUsers.DataSource = this.bsBanks;
                this.lbBancos.SelectedIndex = -1;
            }
            catch (Exception e)
            {

            }
        }

        private void loadCategories()
        {
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest("api/categoria", RestSharp.Method.GET);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                List<Categorias> categorias = client.Execute<List<Categorias>>(request).Data;

                this.bsCategories.DataSource = categorias;
                lbCategories.DataSource = this.bsCategories;
                this.lbCategories.SelectedIndex = -1;
            }
            catch (Exception e)
            {

            }
        }

        private void loadTipopayment()
        {
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest("api/tipopayment", RestSharp.Method.GET);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                List<Tipopayment> tipopayments = client.Execute<List<Tipopayment>>(request).Data;

                this.bsTipopayment.DataSource = tipopayments;
                lbListaTipopagamentos.DataSource = this.bsTipopayment;
                this.lbListaTipopagamentos.SelectedIndex = -1;

                this.bsTipopayment.DataSource = tipopayments;
                cbTipoTransacoes.DataSource = this.bsTipopayment;
            }
            catch (Exception e)
            {

            }
        }

        private void loadBanks()
        {
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest("api/banco", RestSharp.Method.GET);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                List<Banco> bancos = client.Execute<List<Banco>>(request).Data;

                this.bsBanks.DataSource = bancos;
                lbBancos.DataSource = this.bsBanks;
                this.lbBancos.SelectedIndex = -1;

                this.bsBanksCb.DataSource = bancos;
                cbBanksTransactions.DataSource = this.bsBanksCb;
            }
            catch (Exception e)
            {

            }
        }

        private void loadProfile()
        {
            this.lbNomePassword.Text = adminLogged.nome;
            this.lbEmailPassword.Text = adminLogged.email;

        }

        private void loadManageAdmins()
        {
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest("api/admin", RestSharp.Method.GET);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                List<Admin> admins = client.Execute<List<Admin>>(request).Data;

                this.bsAdmins.DataSource = admins;
                lbAdmins.DataSource = this.bsAdmins;
            }
            catch (Exception e)
            {

            }
        }

        private void showSelectedAdmin()
        {
            if (lbAdmins.SelectedIndex > -1)
            {
                Admin admin = (Admin)lbAdmins.SelectedItem;

            }

        }

        private void enableDisableAdmin()
        {
            int selected = lbAdmins.SelectedIndex;
            if (selected > -1)
            {
                Admin admin = (Admin)lbAdmins.SelectedItem;
                try
                {
                    var client = new RestSharp.RestClient(baseURI);
                    var request = new RestSharp.RestRequest($"api/admin/{admin.id}", RestSharp.Method.PATCH);

                    request.AddHeader("content-type", "application/x-www-form-urlencoded");
                    request.AddParameter("application/x-www-form-urlencoded",
                        $"is_active={(admin.is_active == 0 ? 1 : 0)}", ParameterType.RequestBody);
                    request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                    RestSharp.IRestResponse response = client.Execute(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        ((Admin)bsAdmins[selected]).is_active = admin.is_active == 0 ? 1 : 0;
                        bsAdmins.ResetBindings(false);
                        MessageBox.Show($"Admin {admin.nome} {(admin.is_active == 0 ? "Disabled" : "Enabled")}");
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong with the request");
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("Something went wrong with the request");
                }
            }
        }

        private void deleteSelectedAdmin()
        {
            int selected = lbAdmins.SelectedIndex;
            if (selected > -1)
            {
                Admin admin = (Admin)lbAdmins.SelectedItem;
                try
                {
                    var client = new RestSharp.RestClient(baseURI);
                    var request = new RestSharp.RestRequest($"api/admin/{admin.id}", RestSharp.Method.DELETE);
                    request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                    RestSharp.IRestResponse response = client.Execute(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        bsAdmins.Remove(admin);
                        bsAdmins.ResetBindings(false);
                        MessageBox.Show($"Admin {admin.nome} Removed with success");
                    }
                    else
                    {
                        MessageBox.Show("Something went wrong with the request");
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("Something went wrong with the request");
                }
            }
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

        private bool checkValuesForAdminCreation(String email, String nome, String password)
        {
            if (email.Length > 0 || password.Length > 0 || nome.Length > 0)
            {
                if (isValidEmail(email))
                {
                    return true;
                }
                else
                {
                    lbErroCreateAdmin.Text = "The email is not valid";
                }
            }
            else
            {
                lbErroCreateAdmin.Text = "The fields can't be empty!";
            }
            return false;
        }

        private void createAdmin(String email, String nome, String password)
        {
            Admin admin = new Admin
            {
                id = 0,
                nome = nome,
                email = email,
                password = password,
                is_active = 1
            };

            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest("api/admin", RestSharp.Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(admin);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    admin = deserial.Deserialize<Admin>(response);

                    if (admin != null)
                    {
                        this.bsAdmins.Add(admin);
                        bsAdmins.ResetBindings(false);
                        lbAdmins.DataSource = this.bsAdmins;

                        tbEmailCreate.Text = "";
                        tbNameCreate.Text = "";
                        tbPasswordCreate.Text = "";

                        MessageBox.Show("Admin added with success");
                    }
                }
                else
                {
                    lbErroCreateAdmin.Text = "The email is already in use";
                }
            }
            catch (Exception e)
            {

            }
        }

        private bool checkIfPasswords(string nova, string novaConfirmacao, string antiga)
        {
            if (nova.CompareTo(novaConfirmacao) == 0)
            {
                if (nova.CompareTo(antiga) != 0)
                {
                    if (nova.Length > 0)
                    {
                        return true;
                    }
                    else
                    {
                        lbErrosPassword.Text = "The password can't be empty";
                    }
                }
                else
                {
                    lbErrosPassword.Text = "The password must be diff from previous";
                }
            }
            else
            {
                lbErrosPassword.Text = "The passwords doesn't match";
            }

            return false;
        }


        private void changePassword(string nova, string antiga)
        {
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/admin/changePassword", RestSharp.Method.POST);

                Password pass = new Password { nova = nova, antiga = antiga };
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(pass);

                RestSharp.IRestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    tbNovaPassword.Text = "";
                    tbConfirmarPassword.Text = "";
                    tbOldPassword.Text = "";
                    MessageBox.Show("Password edited with success");
                }
                else
                {
                    MessageBox.Show("The passwords did´t match");
                    Admin a = this.adminLogged;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Something went wrong with the request");
            }
        }

        private bool checkBankFields(string ip, string name, int percentage, int maxDebitLimit)
        {
            if (ip.Length >= 9 && ip.Length <= 50)
            {
                if (name.Length >= 3 && name.Length <= 25)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("The name must have 3 to 25 digits!");
                }
            }
            else
            {
                MessageBox.Show("The ip must have 9 to 50 digits!");
            }
            return false;
        }

        private void updateBank(int selected, string ip, string name, int percentage, int maxDebitLimit)
        {
            try
            {
                Banco banco = (Banco)bsBanks[selected];
                banco.ip = ip;
                banco.name = name;
                banco.percentagem = percentage;
                if (cbMaxBankDetail.Checked)
                {
                    banco.max_debit_limit = -1;
                }
                else
                {
                    banco.max_debit_limit = maxDebitLimit;
                }
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/banco/{banco.id}", RestSharp.Method.PUT);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(banco);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    banco = deserial.Deserialize<Banco>(response);

                    if (banco != null)
                    {
                        ((Banco)bsBanks[selected]).ip = banco.ip;
                        ((Banco)bsBanks[selected]).name = banco.name;
                        ((Banco)bsBanks[selected]).percentagem = banco.percentagem;
                        ((Banco)bsBanks[selected]).max_debit_limit = banco.max_debit_limit;
                        bsBanks.ResetBindings(false);

                        MessageBox.Show("Bank updated with success");
                    }
                }
                else
                {
                    lbErroCreateAdmin.Text = "Something went wrong!";
                }

            }
            catch (Exception e)
            {

            }
        }

        private void createBank(string ip, string name, int percentage, int maxDebitLimit)
        {
            try
            {
                if (cbMaxBankCreate.Checked)
                {
                    maxDebitLimit = -1;
                }
                Banco banco = new Banco
                {
                    id = 0,
                    ip = ip,
                    name = name,
                    percentagem = percentage,
                    max_debit_limit = maxDebitLimit,
                    isDeleted = false
                };
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/banco", RestSharp.Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(banco);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    banco = deserial.Deserialize<Banco>(response);

                    if (banco != null)
                    {
                        this.bsBanks.Add(banco);
                        bsBanks.ResetBindings(false);
                        lbBancos.DataSource = this.bsBanks;

                        cbMaxBankCreate.Checked = false;
                        tbIpBankCreate.Text = "";
                        tbNameBankCreate.Text = "";
                        tbNumMaxDebitBankCreate.Value = 1;
                        tbNumPercentageBankCreate.Value = 0;

                        MessageBox.Show("Bank created!");
                    }
                }
                else
                {
                    lbErroCreateAdmin.Text = "Something went wrong!";
                }
            }
            catch (Exception e)
            {

            }
        }

        private void fillUpdateBank(Banco banco)
        {
            tbIpBanco.Text = banco.ip;
            tbNameBank.Text = banco.name;
            tbNumPercentageBank.Value = banco.percentagem;
            if (banco.max_debit_limit == -1)
            {
                cbMaxBankDetail.Checked = true;
                tbNumMaxDebitLimitBank.Value = 1;
            }
            else
            {
                tbNumMaxDebitLimitBank.Value = banco.max_debit_limit;
                cbMaxBankDetail.Checked = false;
            }
            lbBlockBank.Text = (banco.isDeleted ? "Blocked" : "");
            btBlockBank.Text = (banco.isDeleted ? "Enable Bank" : "Block Bank");
        }

        private void blockBank(int selected)
        {
            try
            {
                Banco banco = (Banco)bsBanks[selected];
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/banco/block/{banco.id}", RestSharp.Method.PATCH);

                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    bool isDeleted = deserial.Deserialize<bool>(response);
                    ((Banco)bsBanks[selected]).isDeleted = isDeleted;
                    bsBanks.ResetBindings(false);
                    lbBlockBank.Text = (isDeleted ? "Blocked" : "");
                    btBlockBank.Text = (isDeleted ? "Enable Bank" : "Block Bank");
                }
                else
                {
                    MessageBox.Show("Something went wrong with the request");
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Something went wrong with the request");
            }
        }

        private void checkConnection(int selected)
        {
            try
            {
                Banco banco = (Banco)bsBanks[selected];
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/banco/checkConnection/{banco.id}", RestSharp.Method.GET);

                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MessageBox.Show("The connection is GOOD!");
                }
                else
                {
                    MessageBox.Show("There is no connection :(");
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("There is no connection :(");
            }
        }

        private bool checkCategoryValues(string name)
        {
            if (name.Length >= 3 && name.Length <= 20)
            {
                return true;
            }
            else
            {
                MessageBox.Show("The name must have 3 to 20 digits!");
            }
            return false;
        }

        private bool checkTipopagamentoValues(string code, string name)
        {
            if (name.Length >= 3 && name.Length <= 20)
            {
                if (code.Length <= 20)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("The code can't be bigger than 20 digits!");
                }
            }
            else
            {
                MessageBox.Show("The name must have 3 to 20 digits!");
            }
            return false;
        }

        private void createCategory(string name)
        {
            try
            {
                Categorias categoria = new Categorias
                {
                    id = 0,
                    nome = name,
                    deleted_at = false
                };
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest("api/categoria", RestSharp.Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(categoria);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    categoria = deserial.Deserialize<Categorias>(response);

                    if (categoria != null)
                    {
                        this.bsCategories.Add(categoria);
                        bsCategories.ResetBindings(false);
                        lbCategories.DataSource = this.bsCategories;

                        tbNameCreateCategory.Text = "";

                        MessageBox.Show("Category created!");
                    }
                }
                else
                {
                    if (response.StatusCode == (HttpStatusCode)422)
                    {
                        JsonDeserializer deserial = new JsonDeserializer();
                        Error error = deserial.Deserialize<Error>(response);
                        MessageBox.Show(error.error);
                    }
                    else
                    {
                        MessageBox.Show("Something happen with the DB");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Something happen with the DB");
            }
        }

        private void createTipopagamento(string code, string name)
        {
            try
            {
                Tipopayment tipopayment = new Tipopayment
                {
                    id = 0,
                    code = code,
                    name = name,
                    deleted_at = false
                };
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest("api/tipopayment", RestSharp.Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(tipopayment);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    tipopayment = deserial.Deserialize<Tipopayment>(response);

                    if (tipopayment != null)
                    {
                        this.bsTipopayment.Add(tipopayment);
                        bsTipopayment.ResetBindings(false);
                        lbListaTipopagamentos.DataSource = this.bsTipopayment;

                        tvCreateCodeTipopagamento.Text = "";
                        tvCreateNameTipopagamento.Text = "";

                        MessageBox.Show("Payment Type created!");
                    }
                }
                else
                {
                    if (response.StatusCode == (HttpStatusCode)422)
                    {
                        JsonDeserializer deserial = new JsonDeserializer();
                        Error error = deserial.Deserialize<Error>(response);
                        MessageBox.Show(error.error);
                    }
                    else
                    {
                        MessageBox.Show("Something happen with the DB");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Something happen with the DB");
            }
        }

        private void editCategory(int selected, string name, bool deleted_at)
        {
            try
            {
                Categorias categoria = (Categorias)bsCategories[selected];
                categoria.nome = name;
                categoria.deleted_at = deleted_at;
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/categoria/{categoria.id}", RestSharp.Method.PUT);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(categoria);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    categoria = deserial.Deserialize<Categorias>(response);

                    if (categoria != null)
                    {
                        ((Categorias)bsCategories[selected]).nome = categoria.nome;
                        ((Categorias)bsCategories[selected]).deleted_at = categoria.deleted_at;
                        bsCategories.ResetBindings(false);

                        MessageBox.Show("Category updated with success");
                    }
                }
                else
                {
                    if (response.StatusCode == (HttpStatusCode)422)
                    {
                        JsonDeserializer deserial = new JsonDeserializer();
                        Error error = deserial.Deserialize<Error>(response);
                        MessageBox.Show(error.error);
                    }
                    else
                    {
                        MessageBox.Show("Something happen with the DB");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Something happen with the DB");
            }
        }

        private void editTipopagamento(int selected, string name, bool deleted_at)
        {
            try
            {
                Tipopayment tipopayment = (Tipopayment)bsTipopayment[selected];
                tipopayment.name = name;
                tipopayment.deleted_at = deleted_at;
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/tipopayment/{tipopayment.id}", RestSharp.Method.PUT);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(tipopayment);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    tipopayment = deserial.Deserialize<Tipopayment>(response);

                    if (tipopayment != null)
                    {
                        ((Tipopayment)bsTipopayment[selected]).name = tipopayment.name;
                        ((Tipopayment)bsTipopayment[selected]).deleted_at = tipopayment.deleted_at;
                        bsTipopayment.ResetBindings(false);

                        MessageBox.Show("Payment type updated with success");
                    }
                }
                else
                {
                    if (response.StatusCode == (HttpStatusCode)422)
                    {
                        JsonDeserializer deserial = new JsonDeserializer();
                        Error error = deserial.Deserialize<Error>(response);
                        MessageBox.Show(error.error);
                    }
                    else
                    {
                        MessageBox.Show("Something happen with the DB");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Something happen with the DB");
            }
        }

        private void getUsersFromBank(int idBanco)
        {
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/banco/users/{idBanco}", RestSharp.Method.GET);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                List<User> users = client.Execute<List<User>>(request).Data;

                if (users != null)
                {
                    this.bsUsers.DataSource = users;
                    lbUsersManageUsers.DataSource = this.bsUsers;
                    //banco sem dados, testar para ver a resposta
                    //por label a falar da connecao ao banco
                    gbCreateUser.Enabled = true;
                }
                else
                {
                    this.bsUsers.DataSource = null;
                    lbUsersManageUsers.DataSource = this.bsUsers;
                    gbCreateUser.Enabled = false;
                }
            }
            catch (Exception e)
            {
                this.bsUsers.DataSource = null;
                lbUsersManageUsers.DataSource = this.bsUsers;
                gbCreateUser.Enabled = false;
                MessageBox.Show("No internet connection or something is wrong with the ip.");
            }
        }

        private bool checkForUserData(string name, string email, string phone_number, string password, int code)
        {
            if (name.Length > 3 && name.Length < 50)
            {
                if (isValidEmail(email))
                {
                    if (phone_number.Length == 9)
                    {
                        if (phone_number[0] == '9')
                        {
                            if (password.Length > 3 && password.Length < 30)
                            {
                                if (code.ToString().Length == 4)
                                {
                                    return true;
                                }
                                else
                                {
                                    MessageBox.Show("The code must be 4 digits long");
                                }
                            }
                            else
                            {
                                MessageBox.Show("The password must be 3 to 50 in lenght");
                            }
                        }
                        else
                        {
                            MessageBox.Show("The phone number must be PT 9xxxxxxxx");
                        }
                    }
                    else
                    {
                        MessageBox.Show("The phone number must be PT 9xxxxxxxx");
                    }
                }
                else
                {
                    MessageBox.Show("The email is not valid");
                }
            }
            else
            {
                MessageBox.Show("The name must be 3 to 50 in lenght");
            }
            return false;
        }

        private void createUser(int idBanco, User user, Phone phone)
        {
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/banco/users/{idBanco}", RestSharp.Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);
                request.AddJsonBody(new UserPhone { user = user, phone = phone });

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    user = deserial.Deserialize<User>(response);
                    this.bsUsers.Add(user);
                    bsUsers.ResetBindings(false);
                    lbUsersManageUsers.DataSource = this.bsUsers;

                    clearDadosUser();
                    MessageBox.Show("User created");
                }
                else
                {
                    MessageBox.Show("Something went wrong with request, number already in use!");
                }
            }
            catch (Exception e)
            {
                string a = e.HelpLink;
            }
        }

        private void getAllTransactionsToExcell(int escolha, string value)
        {
            try
            {
                var client = new RestSharp.RestClient(baseURI);
                var request = new RestSharp.RestRequest($"api/transacao/all", RestSharp.Method.GET);
                switch (escolha)
                {
                    case 0:
                        // ALL
                        request = new RestSharp.RestRequest($"api/transacao/all", RestSharp.Method.GET);
                        break;
                    case 1:
                        //type C D
                        try
                        {
                            request = new RestSharp.RestRequest($"api/transacao/operationType/{Int32.Parse(value)}", RestSharp.Method.GET);
                        }catch(Exception e)
                        {
                            MessageBox.Show("Soemthing went wrong");
                            return;
                        }
                        break;
                    case 2:
                        //VCARD USER ....
                        request = new RestSharp.RestRequest($"api/transacao/type/{value}", RestSharp.Method.GET);
                        break;
                    case 3:
                        //just from a bank
                        request = new RestSharp.RestRequest($"api/transacao/bank/{value}", RestSharp.Method.GET);
                        break;
                }
                
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", "Bearer " + this.adminLogged.accessToken);

                RestSharp.IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonDeserializer deserial = new JsonDeserializer();
                    List<nameTransacoes> transacoes = client.Execute<List<nameTransacoes>>(request).Data;
                    SaveFileDialog openFileDialog1 = new SaveFileDialog();
                    openFileDialog1.FileName = "myxml.xls";
                    openFileDialog1.Filter = "XLS Files| *.xls";
                    openFileDialog1.CheckFileExists = false;
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var path = openFileDialog1.FileName;
                        ExcellHandler.WriteToExcelFile(transacoes, path);
                        MessageBox.Show("sucess");
                    }
                }
                else
                {
                    MessageBox.Show(response.Content);
                }
            }
            catch (Exception e)
            {
                string a = e.HelpLink;
            }
        }

        private void clearDadosUser()
        {
            tbNameCreateUser.Text = "";
            tbEmailCreateUser.Text = "";
            tbPhoneCreateUser.Text = "";
            tbPasswordCreateUser.Text = "";
            tbNumCodeCreateUser.Value = 1000;
        }

        private void fillUpdateCategory(Categorias categoria)
        {
            tbNameEditCategory.Text = categoria.nome;
            cbEnableCategory.Checked = categoria.deleted_at;
        }

        private void fillUpdateTipopayment(Tipopayment tipopayment)
        {
            tvNameTipopagamento.Text = tipopayment.name;
            cbDisTipopagamentos.Checked = tipopayment.deleted_at;
        }

        private void btReloadAdminList_Click(object sender, EventArgs e)
        {
            loadManageAdmins();
        }

        private void lbAdmins_SelectedIndexChanged(object sender, EventArgs e)
        {
            showSelectedAdmin();
        }

        private void btBlockAdmin_Click(object sender, EventArgs e)
        {
            enableDisableAdmin();
        }

        private void btDeleteAdmin_Click(object sender, EventArgs e)
        {
            deleteSelectedAdmin();
        }

        private void btCreateAdmin_Click(object sender, EventArgs e)
        {
            lbErroCreateAdmin.Text = "";
            if (checkValuesForAdminCreation(tbEmailCreate.Text, tbNameCreate.Text, tbPasswordCreate.Text))
            {
                createAdmin(tbEmailCreate.Text, tbNameCreate.Text, tbPasswordCreate.Text);
            }
        }

        private void gbAdmin_Enter(object sender, EventArgs e)
        {

        }

        private void btAlterarPassword_Click(object sender, EventArgs e)
        {
            if (checkIfPasswords(tbNovaPassword.Text, tbConfirmarPassword.Text, tbOldPassword.Text))
            {
                changePassword(tbNovaPassword.Text, tbOldPassword.Text);
            }
        }

        private void btReloadBanks_Click(object sender, EventArgs e)
        {
            loadBanks();
        }

        private void btUpdateBank_Click(object sender, EventArgs e)
        {
            int select = lbBancos.SelectedIndex;
            if (select >= 0)
            {
                if (checkBankFields(tbIpBanco.Text, tbNameBank.Text, (int)tbNumPercentageBank.Value, (int)tbNumMaxDebitLimitBank.Value))
                {
                    updateBank(select, tbIpBanco.Text, tbNameBank.Text, (int)tbNumPercentageBank.Value, (int)tbNumMaxDebitLimitBank.Value);
                }
            }
            else
            {
                MessageBox.Show("Must select a bank so you can change it");
            }
        }

        private void btBlockBank_Click(object sender, EventArgs e)
        {
            int select = lbBancos.SelectedIndex;
            if (select >= 0)
            {
                blockBank(select);
            }
        }

        private void btTestConnBank_Click(object sender, EventArgs e)
        {
            int select = lbBancos.SelectedIndex;
            if (select >= 0)
            {
                checkConnection(select);
            }
        }

        private void lbBancos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbBancos.SelectedIndex >= 0)
            {
                gbBankDetail.Enabled = true;
                Banco banco = (Banco)bsBanks[lbBancos.SelectedIndex];
                fillUpdateBank(banco);
            }
            else
            {
                gbBankDetail.Enabled = false;
                tbIpBanco.Text = "";
                tbNameBank.Text = "";
                tbNumPercentageBank.Value = 1;
                tbNumMaxDebitLimitBank.Value = 1;
                cbMaxBankDetail.Checked = false;
                lbBlockBank.Text = "";
                btBlockBank.Text = "Block Bank";
            }

        }

        private void btCreate_Click(object sender, EventArgs e)
        {
            if (checkBankFields(tbIpBankCreate.Text, tbNameBankCreate.Text, (int)tbNumPercentageBankCreate.Value, (int)tbNumMaxDebitBankCreate.Value))
            {
                createBank(tbIpBankCreate.Text, tbNameBankCreate.Text, (int)tbNumPercentageBankCreate.Value, (int)tbNumMaxDebitBankCreate.Value);
            }
        }

        private void btReloadCategories_Click(object sender, EventArgs e)
        {
            loadCategories();
        }

        private void lbCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbCategories.SelectedIndex >= 0)
            {
                gbEditCategories.Enabled = true;
                Categorias categoria = (Categorias)bsCategories[lbCategories.SelectedIndex];
                fillUpdateCategory(categoria);
            }
            else
            {
                gbEditCategories.Enabled = false;
                tbNameEditCategory.Text = "";
                cbEnableCategory.Checked = false;
            }
        }

        private void btCreateCategory_Click(object sender, EventArgs e)
        {
            if (checkCategoryValues(tbNameCreateCategory.Text))
            {
                createCategory(tbNameCreateCategory.Text);
            }

        }

        private void btEditCategory_Click(object sender, EventArgs e)
        {
            int select = lbCategories.SelectedIndex;
            if (select >= 0)
            {
                if (checkCategoryValues(tbNameEditCategory.Text))
                {
                    editCategory(select, tbNameEditCategory.Text, cbEnableCategory.Checked);
                }
            }
            else
            {
                MessageBox.Show("Must select a Category so you can change it");
            }
        }

        private void btCreateUser_Click(object sender, EventArgs e)
        {
            int selected = cbBancoManageUsers.SelectedIndex;
            if (selected >= 0)
            {
                if (checkForUserData(tbNameCreateUser.Text, tbEmailCreateUser.Text, tbPhoneCreateUser.Text, tbPasswordCreateUser.Text, (int)tbNumCodeCreateUser.Value))
                {
                    Banco banco = (Banco)this.bsBanks[selected];
                    User user = new User
                    {
                        id = 0,
                        name = tbNameCreateUser.Text,
                        email = tbEmailCreateUser.Text,
                        password = tbPasswordCreateUser.Text
                    };
                    Phone phone = new Phone
                    {
                        id_user = 0,
                        phone_number = tbPhoneCreateUser.Text,
                        code = (int)tbNumCodeCreateUser.Value

                    };

                    createUser(banco.id, user, phone);
                }
            }
            else
            {
                MessageBox.Show("Need to select a server first");
            }
        }

        private void cbBancoManageUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = cbBancoManageUsers.SelectedIndex;
            if (selected >= 0)
            {
                Banco banco = (Banco)this.bsBanks[selected];
                getUsersFromBank(banco.id);
            }
        }

        private void btReloadTipopagamentos_Click(object sender, EventArgs e)
        {
            loadTipopayment();
        }

        private void lbListaTipopagamentos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbListaTipopagamentos.SelectedIndex >= 0)
            {
                gbEditTipopagamento.Enabled = true;
                Tipopayment tipopayment = (Tipopayment)bsTipopayment[lbListaTipopagamentos.SelectedIndex];
                fillUpdateTipopayment(tipopayment);
            }
            else
            {
                gbEditTipopagamento.Enabled = false;
                tvNameTipopagamento.Text = "";
                cbDisTipopagamentos.Checked = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int select = lbListaTipopagamentos.SelectedIndex;
            if (select >= 0)
            {
                if (checkCategoryValues(tvNameTipopagamento.Text))
                {
                    editTipopagamento(select, tvNameTipopagamento.Text, cbDisTipopagamentos.Checked);
                }
            }
            else
            {
                MessageBox.Show("Must select a Payment Type so you can change it");
            }
        }

        private void btCreateTipopagamento_Click(object sender, EventArgs e)
        {
            if (checkTipopagamentoValues(tvCreateCodeTipopagamento.Text, tvCreateNameTipopagamento.Text))
            {
                createTipopagamento(tvCreateCodeTipopagamento.Text, tvCreateNameTipopagamento.Text);
            }
        }

        private void btExportAllBanks_Click(object sender, EventArgs e)
        {
            getAllTransactionsToExcell(0,"nada");
        }

        private void btExportAllBanksTipo_Click(object sender, EventArgs e)
        {
            getAllTransactionsToExcell(1, cbCreditDebitTransaction.SelectedIndex + "");
        }

        private void btExportAllBanksEntenty_Click(object sender, EventArgs e)
        {
            getAllTransactionsToExcell(2, ((Tipopayment)bsTipopayment[cbTipoTransacoes.SelectedIndex]).code);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            getAllTransactionsToExcell(3, ((Banco)bsBanksCb[cbBanksTransactions.SelectedIndex]).id + "");
        }
    }
}
