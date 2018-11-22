using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Aras.IOM;
using System.Web;
using System.Configuration;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            updateLoginDialog();
        }

        private void updateLoginDialog()
        {
            
            string value = ConfigurationManager.AppSettings["url"];
            url.Text = value;
            string Userid = ConfigurationManager.AppSettings["userid"];
            user_id.Text = Userid;
            string Database = ConfigurationManager.AppSettings["database"];
            database.Text = Database;
            string filepath = ConfigurationManager.AppSettings["filepath"];
            folderBrowserDialog1.SelectedPath = filepath;

            //ConfigurationManager.OpenExeConfiguration();
        }
        HttpServerConnection conn = null;
        private void button1_Click(object sender, EventArgs e)
        {
            
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && date.Length >0)
            {
                String htmlfilepath = Program.ListAllMdifiedObjects(date, folderBrowserDialog1.SelectedPath, conn);
                System.Uri uri = new System.Uri("file:///"+htmlfilepath);
                webBrowser1.Url = uri;
            }
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["filepath"].Value = folderBrowserDialog1.SelectedPath;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
           // Form1.
           // this.Close();
        }

        private void cancel_btn_Click(object sender, EventArgs e)
        {
            if (date.Length > 0)
            {
                String htmlfilepath = Program.ListAllMdifiedObjects(date, folderBrowserDialog1.SelectedPath, conn);
                System.Uri uri = new System.Uri("file:///" + htmlfilepath);
                webBrowser1.Url = uri;
            }
        }
        String date = DateTime.Now.ToShortDateString();

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            date = monthCalendar1.SelectionRange.Start.ToShortDateString();
        }
        String ArasURL = "";
        private void button1_Click_1(object sender, EventArgs e)
        {
            ArasURL = url.Text;
            String userid = user_id.Text;
            string DataBaseName = database.Text;
            String usePass = password.Text;
            if (String.IsNullOrEmpty(ArasURL) || String.IsNullOrEmpty(DataBaseName) || String.IsNullOrEmpty(usePass) || String.IsNullOrEmpty(userid))
            {
                MessageBox.Show("Please provide all the information to login");
                return;
            }

            conn = Program.login(ArasURL, DataBaseName, userid, usePass);
            if (conn != null)
            {
                monthCalendar1.Visible = true;
                button1.Visible = true;
                ok_btn.Visible = true;
                refresh_btn.Visible = true;
                //userlabel.Visible = true;
                //userlist.Visible = true;
                Configuration config=ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["url"].Value = url.Text;
                config.AppSettings.Settings["database"].Value = database.Text;
                config.AppSettings.Settings["userid"].Value = user_id.Text; 
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                //ConfigurationManager.AppSettings["url"] = url.Text;
                //ConfigurationManager.AppSettings["database"] = database.Text;
                //ConfigurationManager.AppSettings["userid"] = user_id.Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void url_TextLostFocus(object sender, EventArgs e)
        {
            //database.Text = "";
           // database.Items.Clear();

           // if (String.IsNullOrEmpty(url.Text))
           // {
           //     return;
           // }
           // List<String> DBList = Program.getAllDatabases(url.Text);
            
           // //HttpContext.Current.Server.MapPath(ArasURL);
           //// webBrowser1.Url;

           // foreach (String DBName in DBList)
           // {
           //     //database.
           //     database.Items.Add(DBName);
           // }
            


        }

        private void password_TextChanged(object sender, EventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    button1_Click_1(sender, e); //here LoginButton_Click is click eventhandler
            //}
        }

        private void database_SelectedIndexChanged(object sender, EventArgs e)
        {
            monthCalendar1.Visible = false;
            button1.Visible = true;
            ok_btn.Visible = false;
            refresh_btn.Visible = false;
            //userlabel.Visible = true;
            //userlist.Visible = true;
        }

        private void database_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            database.Text = "";
            Form actfrm = Form1.ActiveForm;

            var currcursor = actfrm.Cursor;
            actfrm.Cursor = System.Windows.Forms.Cursors.AppStarting;
             try
            {   //actfrm.Cursor = System.Windows.Forms.Cursor ;
                errorProvider1.Clear();
                if (conn != null)
                {
                    conn.Logout();
                }
                database.Items.Clear();
                String ArasURL = url.Text;
                if (String.IsNullOrEmpty(ArasURL))
                {
                    MessageBox.Show("Enter URL...");
                    return;
                }
                String[] urlarray = ArasURL.Split('/');

                String ServerName = urlarray[2];
                String Site = urlarray[3];

                String urlval = @"http://" + ServerName + "/" + Site;

                HttpServerConnection conn1 = IomFactory.CreateHttpServerConnection(urlval);
                String[] DBNames = conn1.GetDatabases();

                foreach (String DBName in DBNames)
                {
                    database.Items.Add(DBName);
                }
                
                
            }
            catch(Exception ex)
            {
                actfrm.Cursor = currcursor;
                errorProvider1.SetError(url,"Error while getting DBs for this URL \n"+ex.ToString());
                //throw ex;
            }
            actfrm.Cursor = currcursor;
        }
        // System.Windows.Forms.MonthCalendar 
    }
}
