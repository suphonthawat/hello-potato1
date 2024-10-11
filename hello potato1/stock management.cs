using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace hello_potato1
{
    public partial class stock_management : Form
    {
        string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";

        public stock_management()
        {
            InitializeComponent();
            LoadData(); // Load data when the form is initialized
        }

        private void stock_management_Load(object sender, EventArgs e)
        {
            LoadData(); // Load data when the form is loaded
        }

        private void LoadData()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM stock";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            admin f1 = new admin();
            f1.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
            if (open.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(open.FileName);
            }
        }

        private void AddItemToCategory(string category)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // ตรวจสอบว่าค่า quantity เป็นตัวเลขและไม่ติดลบ
                if (!int.TryParse(textBox4.Text, out int quantity) || quantity < 0)
                {
                    MessageBox.Show("กรุณาใส่จำนวนสินค้าเป็นตัวเลขและต้องไม่น้อยกว่า 0");
                    return;
                }

                string query = "INSERT INTO stock (name, category, price, quantity, description, photo) VALUES (@name, @category, @price, @quantity, @description, @photo)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", textBox2.Text);
                cmd.Parameters.AddWithValue("@category", category);
                cmd.Parameters.AddWithValue("@price", textBox3.Text);
                cmd.Parameters.AddWithValue("@quantity", quantity); // ใช้ค่าที่ตรวจสอบแล้วจากการแปลง

                // Handle description with a fixed length
                string description = textBox1.Text;
                int maxLength = 255; // Example max length for description
                if (description.Length > maxLength)
                {
                    description = description.Substring(0, maxLength);
                }
                cmd.Parameters.AddWithValue("@description", description);

                MemoryStream ms = new MemoryStream();
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat);
                }
                byte[] img = ms.ToArray();
                cmd.Parameters.AddWithValue("@photo", img);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("เพิ่มสินค้าเรียบร้อย!");
                    ClearForm(); // Clear form after adding the item
                    LoadData(); // Refresh data in DataGridView
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void ClearForm()
        {
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox1.Clear();
            pictureBox1.Image = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (AreTextBoxesEmpty())
            {
                MessageBox.Show("Please enter product information.");
                return;
            }

            AddItemToCategory("menu");
        }

        

        private bool AreTextBoxesEmpty()
        {
            return string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox4.Text) || string.IsNullOrWhiteSpace(textBox1.Text);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                    textBox2.Text = row.Cells["name"].Value.ToString();
                    textBox3.Text = row.Cells["price"].Value.ToString();
                    textBox4.Text = row.Cells["quantity"].Value.ToString();
                    textBox1.Text = row.Cells["description"].Value.ToString(); // Load description

                    if (row.Cells["photo"].Value != DBNull.Value)
                    {
                        byte[] img = (byte[])row.Cells["photo"].Value;
                        MemoryStream ms = new MemoryStream(img);
                        pictureBox1.Image = Image.FromStream(ms);
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading the image: " + ex.Message);
                    pictureBox1.Image = null;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            
            {
                if (AreTextBoxesEmpty())
                {
                    MessageBox.Show("Please enter product information.");
                    return;
                }

                AddItemToCategory("drink");
            }

        }
    }
}
