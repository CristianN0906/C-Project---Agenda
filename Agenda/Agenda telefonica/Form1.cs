using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Configuration;

namespace Agenda_telefonica
{
    public partial class Form1 : Form
    {
        // TODO in future - connection throuh App.config
        string connConfig = ConfigurationManager.ConnectionStrings["dbConn"].ConnectionString;

        // Declaration of variables
        public string connSettings = @"Server=DESKTOP-4MH1ERN\SQLEXPRESS;Database=Agenda;Trusted_Connection=True"; 
        private SqlDataAdapter daDB;
        private DataSet dsDB;
        private SqlConnection conn;
        UserPreferences uPref = new UserPreferences();
        BinaryFormatter binFormat = new BinaryFormatter();

        public Form1()
        {
            InitializeComponent();
            dataGridView1.ReadOnly = false;

            // Verify if "user.dat" exists and deserialize the preferences stored there
            if (File.Exists(@"user.dat"))
            {
                binFormat = new BinaryFormatter();

                object deS = binFormat.Deserialize(new FileStream("user.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                uPref = (UserPreferences)deS;

                // Buttons color
                if (!uPref.preferences[0].IsEmpty)
                    foreach (Button btn in Controls.OfType<Button>())
                        btn.BackColor = uPref.preferences[0];

                //backcolor of the form
                if (!uPref.preferences[1].IsEmpty)
                    this.BackColor = uPref.preferences[1];

                // datagridview background color
                if (!uPref.preferences[2].IsEmpty)
                    dataGridView1.BackgroundColor = uPref.preferences[2];

            }

            timer1.Start();
        }

        // BUTTONS

        // Load Button
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                string command = "SELECT * FROM Persoane";

                if (dsFillDB(command))
                {
                    SqlCommandBuilder cmdBldr = new SqlCommandBuilder(daDB);

                    dataGridView1.DataSource = dsDB;
                    dataGridView1.DataMember = "Person";

                    //Verify if the database is empty
                    if (dataGridView1.RowCount == 1)
                    {
                        DialogResult dialog = MessageBox.Show("DataBase checked!","No subscribers found in DataBase!", MessageBoxButtons.OK);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on Load: " + ex);
            }
        }

        //Save Button
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                daDB.Update(dsDB, "Person");
                MessageBox.Show("DataBase have been updated!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex);
            }
        }

        // MENU
        
        // Customization
        //buttons color
        private void buttonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (Button btn in Controls.OfType<Button>())
                       btn.BackColor = colorDialog1.Color;
                   
                uPref.preferences[0] = colorDialog1.Color;
            }
        }

        //windows color
        private void windowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.BackColor = colorDialog1.Color;
                uPref.preferences[1] = colorDialog1.Color;
            }
        }

        //DGV background-color
        private void dataGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dataGridView1.BackgroundColor = colorDialog1.Color;
                uPref.preferences[2] = colorDialog1.Color;
            }
            
        }
        
        // EXIT
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Do you want to close the program?", "Exit", MessageBoxButtons.YesNo);
            if (dialog == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
                

        //Serialization of information
        private void serializareInformatiiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if ((dsDB != null) || (dataGridView1.RowCount > 1))
                {

                    //string filepathname = "Subscribers_" + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + "." + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + ".xml";
                    string filepath = @"..\..\..\" + "Subscribers_" + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + "." + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + ".xml";

                    

                    //saveFileDialog2.Filter = "File XML| *.xml";
                    //saveFileDialog2.Title = "Save data";
                    //saveFileDialog2.FileName = filepathname;


                    //if (saveFileDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    //{
                        XmlTextWriter xmlSave = new XmlTextWriter(filepath, Encoding.UTF8);
                        xmlSave.Formatting = Formatting.Indented;
                        dsDB.DataSetName = "Data";
                        dsDB.WriteXml(xmlSave);
                        xmlSave.Close();
                        MessageBox.Show("The file was saved!");
                    //}
                    //else
                    //{
                    //    MessageBox.Show("Error: The file was not saved!");
                    //}


                }
                else
                {
                    MessageBox.Show("DataGridView is empty!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error!" + ex);
            }

        }

        // Person Search
        private void cautaPersoanaToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Form formSearch = new Form();
            formSearch.Visible = true;
            formSearch.Location = new Point(100, 100);
            formSearch.Width = 800;
            formSearch.Height = 400;
            formSearch.SuspendLayout();

            DataGridView DGVSearch = new DataGridView();
            DGVSearch.Location = new Point(10, 100);
            DGVSearch.Size = new Size(600, 200);
            DGVSearch.Visible = true;
            DGVSearch.ReadOnly = true;
            formSearch.Controls.Add(DGVSearch);

            TextBox txtSearch = new TextBox();
            txtSearch.Location = new Point(10, 10);
            txtSearch.Visible = true;
            txtSearch.Width = 200;
            formSearch.Controls.Add(txtSearch);

            Button btnSearch = new Button();
            btnSearch.Location = new Point(10, 50);
            btnSearch.Visible = true;
            btnSearch.Text = "Search";
            formSearch.Controls.Add(btnSearch);

            // Create an event when the button is pressed
            //btnSearch.Click += new EventHandler(btnSearch_Click, txtSearch.Text);
            btnSearch.Click += new EventHandler((evsender, ex) => btnSearch_Click(evsender, ex, txtSearch.Text, DGVSearch));
        }

        private void btnSearch_Click(object sender, EventArgs e, string text, DataGridView DGVSearch)
        {
            try
            {
                string command = "SELECT * FROM Persoane WHERE Nume = '" + text + "' OR Prenume = '" + text + "' OR Telefon = '" + text + "' OR Oras = '" + text + "'";

                if (dsFillDB(command))
                {
                    SqlCommandBuilder cmdBldr = new SqlCommandBuilder(daDB);

                    DGVSearch.DataSource = dsDB;
                    DGVSearch.DataMember = "Person";

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on Load: " + ex);
            }
        }

        // method which bring the information from database into DataSource, into DataAdapter
        private bool dsFillDB(string command)
        {
            try
            {
                dataGridView1.DataSource = null;
                conn = new SqlConnection(connSettings);
                dsDB = new DataSet();
                daDB = new SqlDataAdapter(command, connSettings);

                daDB.Fill(dsDB, "Person");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error at connection to DB: " + ex);
                return false;
            }
        }

        // Save preferences when the program is closed
        public void SavePrefIntoFile()
        {
            //The object is saved into a file named user.dat

            using (Stream fStream = new FileStream("user.dat",
                                                    FileMode.Create,
                                                    FileAccess.Write,
                                                    FileShare.ReadWrite))
            {
                binFormat.Serialize(fStream, uPref);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePrefIntoFile();
        }

        //time ticker
        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            this.lblTime.Text = dateTime.ToString();
        }
    }

    [Serializable]
    public class UserPreferences
    {

        public Color[] preferences = new Color[3];
        //preferences[0] saves button BackGround color
        //preferences[1] saves datagridview BackGround color
        //preferences[2] saves Form backGround color
        
        //TODO
        public string font;
                              
        public UserPreferences()
        {
            
        }

    }

    // TODO in future - create a class which store methods to manipulate the database commands
    public class DataBaseWork
    { 
    
    }
}
