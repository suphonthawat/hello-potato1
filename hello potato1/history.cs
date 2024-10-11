using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hello_potato1
{
    public partial class history : Form
    {
        public history()
        {
            InitializeComponent();
            LoadComboBox();
        }

        private MySqlConnection databaseConnection()
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        private void LoadComboBox()
        {
            comboBox1.Items.Add("stock");
            comboBox1.Items.Add("userorder");
            comboBox1.Items.Add("cart");
            comboBox1.Items.Add("membership");
            comboBox1.SelectedIndex = 0; // Default to the first table
        }

        private void LoadData(string tableName, string searchQuery = "")
        {
            using (MySqlConnection conn = databaseConnection())
            {
                string query;

                if (tableName == "userorder")
                {
                    // Only select the relevant columns, excluding 'Receipt'
                    query = "SELECT `id`, `Username`, `Product`,  `quantity`,  `date`, `total`, `status` FROM `userorder`";
                }
                else
                {
                    query = $"SELECT * FROM `{tableName}`";
                }

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    query += $" WHERE CONCAT_WS(' ', {string.Join(", ", GetSearchableColumns(tableName))}) LIKE '%{searchQuery}%'";
                }

                try
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    // Check if the table contains image data
                    if (tableName == "stock" && table.Columns.Contains("ImageColumnName")) // Replace with your actual image column name
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            if (row["ImageColumnName"] != DBNull.Value)
                            {
                                byte[] imgBytes = (byte[])row["ImageColumnName"];
                                using (MemoryStream ms = new MemoryStream(imgBytes))
                                {
                                    row["ImageColumnName"] = Image.FromStream(ms);
                                }
                            }
                        }
                    }

                    dataGridView1.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error");
                }
            }
        }

        private string[] GetSearchableColumns(string tableName)
        {
            switch (tableName)
            {
                case "userorder":
                    return new[] { "id", "Username", "Product", "quantity", "date", "total", "status" };
                case "stock":
                    return new[] { "Name", "Category", "Price", "Quantity" }; // แก้ไขเป็นชื่อคอลัมน์จริงของ stock
                case "cart":
                    return new[] { "Name", "Quantity", "Price", "Username" }; // แก้ไขเป็นชื่อคอลัมน์จริงของ cart
                case "membership":
                    return new[] { "Name", "Email", "Username", "Phonenumber" };
                default:
                    return new[] { "*" };
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData(comboBox1.SelectedItem.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string tableName = comboBox1.SelectedItem.ToString();
            string searchQuery = textBox2.Text;
            LoadData(tableName, searchQuery);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            admin f1 = new admin();
            f1.Show();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void history_Load(object sender, EventArgs e)
        {

        }
    }
}
