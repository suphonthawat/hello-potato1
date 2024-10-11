using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hello_potato1
{
    public partial class stockedit1 : Form
    {
        // Connection string for MySQL database
        private string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;charset=utf8;";

        public stockedit1()
        {
            InitializeComponent();
            LoadData(); // Load data when the form is initialized
        }

        private void LoadData(string search = "")
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM stock";
                    if (!string.IsNullOrEmpty(search))
                    {
                        query += " WHERE name LIKE @search";
                    }
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                textBox2.Text = row.Cells["name"].Value.ToString();
                textBox3.Text = row.Cells["price"].Value.ToString();
                textBox4.Text = row.Cells["quantity"].Value.ToString();
                textBox5.Text = row.Cells["description"].Value.ToString(); // Load description

                // Load image from database
                if (row.Cells["photo"].Value != DBNull.Value && row.Cells["photo"].Value != null)
                {
                    byte[] imageBytes = row.Cells["photo"].Value as byte[]; // Cast value to byte[]
                    if (imageBytes != null)
                    {
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        pictureBox1.Image = null; // Clear image if imageBytes is null
                    }
                }
                else
                {
                    pictureBox1.Image = null; // Clear image if no image is available
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) // ลบแถวที่เลือก
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("กรุณาเลือกแถวที่ต้องการลบ");
                return;
            }

            string nameToDelete = dataGridView1.CurrentRow.Cells["name"].Value.ToString();
            // อาจใช้ ID หรือระบุอื่นที่ไม่ซ้ำกัน
            // string idToDelete = dataGridView1.CurrentRow.Cells["id"].Value.ToString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM stock WHERE name = @name";
                    // ใช้บรรทัดด้านล่างหากใช้คอลัมน์ ID
                    // string query = "DELETE FROM stock WHERE id = @id";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", nameToDelete);
                    // ใช้บรรทัดด้านล่างหากใช้คอลัมน์ ID
                    // cmd.Parameters.AddWithValue("@id", idToDelete);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("ลบแถวที่เลือกสำเร็จ!");
                    }
                    else
                    {
                        MessageBox.Show("ไม่มีแถวใดถูกลบ กรุณาลองใหม่อีกครั้ง");
                    }
                    LoadData(); // รีเฟรชข้อมูล
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ข้อผิดพลาด: " + ex.Message);
                }
            }
        }


        private void button2_Click(object sender, EventArgs e) // Decrease quantity
        {
            UpdateQuantity(-1);
        }

        private void button3_Click(object sender, EventArgs e) // Increase quantity
        {
            UpdateQuantity(1);
        }

        private void UpdateQuantity(int change)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int currentQuantity = Convert.ToInt32(textBox4.Text);
                int newQuantity = currentQuantity + change;

                // Ensure quantity is not negative
                if (newQuantity < 0)
                {
                    MessageBox.Show("Quantity cannot be negative.");
                    return;
                }

                textBox4.Text = newQuantity.ToString();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "UPDATE stock SET quantity = @quantity WHERE name = @name";
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@quantity", newQuantity);
                        cmd.Parameters.AddWithValue("@name", textBox2.Text);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Quantity updated!");
                        LoadData(); // Refresh data
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }


        private void button4_Click(object sender, EventArgs e) // Save
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a row to save changes.");
                return;
            }

            // Ensure quantity is not negative before saving
            int quantity;
            if (!int.TryParse(textBox4.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Quantity cannot be negative. Please enter a valid value.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE stock SET name = @name, price = @price, quantity = @quantity, description = @description WHERE name = @oldName";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", textBox2.Text);
                    cmd.Parameters.AddWithValue("@price", textBox3.Text);
                    cmd.Parameters.AddWithValue("@quantity", quantity);
                    cmd.Parameters.AddWithValue("@description", textBox5.Text); // Save description
                    cmd.Parameters.AddWithValue("@oldName", dataGridView1.CurrentRow.Cells["name"].Value.ToString());
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data saved!");
                    LoadData(); // Refresh data
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Hide();
            admin f1 = new admin();
            f1.Show();
        }

        private void button6_Click(object sender, EventArgs e) // Search
        {
            LoadData(textBox1.Text);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(openFileDialog.FileName);

                    // Set PictureBoxSizeMode to StretchImage
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

                    SaveImageToDatabase(openFileDialog.FileName);
                }
            }
        }

        private void SaveImageToDatabase(string imagePath)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    string query = "UPDATE stock SET photo = @photo WHERE name = @name"; // Update to 'photo'
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@photo", imageBytes); // Update to 'photo'
                    cmd.Parameters.AddWithValue("@name", textBox2.Text);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Image upload!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Optionally trigger search here or via button6_Click
        }

        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }

        private void ResizeFontToFit(TextBox textBox)
        {
            // ตั้งค่าฟอนต์เริ่มต้น
            float fontSize = 12.0f;
            Font font = new Font(textBox.Font.FontFamily, fontSize);

            // วนลูปเพื่อลดขนาดฟอนต์จนกว่าจะแสดงข้อความได้หมดหรือจนกว่าขนาดฟอนต์จะเล็กลงตามที่กำหนด
            while (textBox.CreateGraphics().MeasureString(textBox.Text, font).Width > textBox.Width && fontSize > 6.0f)
            {
                fontSize -= 0.5f; // ลดขนาดฟอนต์ลง
                font = new Font(textBox.Font.FontFamily, fontSize);
            }

            // ตั้งค่าฟอนต์ให้กับ TextBox
            textBox.Font = font;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            ResizeFontToFit(textBox5); // เรียกใช้เมธอดเมื่อมีการเปลี่ยนแปลงข้อความ
        }


        private void textBox5_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void stockedit1_Load(object sender, EventArgs e)
        {

        }
    }
}
