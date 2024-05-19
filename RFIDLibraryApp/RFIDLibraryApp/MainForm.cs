using RFIDLibraryApp.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RFIDLibraryApp
{
    public partial class MainForm : Form
    {
        private ImpinjReader reader;
        private string connectionString = "Data Source=library.db;Version=3;";
        private string scannedRfidTag;

        public MainForm()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "CREATE TABLE IF NOT EXISTS Books (Id INTEGER PRIMARY KEY, Title TEXT, RfidTag TEXT)";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                command.ExecuteNonQuery();
            }
            LoadBooks();
        }

        private void LoadBooks()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Books";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, conn);
                System.Data.DataTable dt = new System.Data.DataTable();
                adapter.Fill(dt);
                dataGridViewBooks.DataSource = dt;
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            reader = new ImpinjReader();
            try
            {
                reader.Connect("hostname"); // Zamijeni "hostname" s IP adresom ili imenom čitača
                Settings settings = reader.QueryDefaultSettings();
                settings.Report.IncludeAntennaPortNumber = true;
                settings.Report.IncludeFirstSeenTime = true;
                settings.Report.IncludePeakRssi = true;
                settings.Report.IncludeSeenCount = true;

                reader.ApplySettings(settings);
                reader.TagsReported += OnTagsReported;
                reader.Start();
            }
            catch (OctaneSdkException ex)
            {
                MessageBox.Show("Error connecting to reader: " + ex.Message);
            }
        }

        private void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            foreach (Tag tag in report)
            {
                scannedRfidTag = tag.Epc.ToString();
                // Prikazivanje pronađenih tagova u MessageBox
                MessageBox.Show($"RFID Tag: {scannedRfidTag}");
            }
            reader.Stop();
            reader.Disconnect();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtTitle.Text) && !string.IsNullOrEmpty(scannedRfidTag))
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Books (Title, RfidTag) VALUES (@Title, @RfidTag)";
                    SQLiteCommand command = new SQLiteCommand(sql, conn);
                    command.Parameters.AddWithValue("@Title", txtTitle.Text);
                    command.Parameters.AddWithValue("@RfidTag", scannedRfidTag);
                    command.ExecuteNonQuery();
                }
                LoadBooks();
            }
            else
            {
                MessageBox.Show("Unesi naslov knjige i skeniraj RFID tag.");
            }
        }
    }
}

