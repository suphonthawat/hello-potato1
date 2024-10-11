using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hello_potato1
{
    public partial class Form3 : Form
    {
        private string name;
        private string price;
        private string quantity;
        private string Username;
        private string Category;
        private byte[] photoBytes;

        public Form3(string id, string Username, string name, string Category, string price, string quantity, byte[] photoBytes)


        {
            InitializeComponent();
            this.Username = Username;
            this.name = name;
            this.Category = Category;
            this.price = price;
            this.quantity = quantity;
            this.photoBytes = photoBytes;

            if (photoBytes != null && photoBytes.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(photoBytes))
                    {
                        Image img = Image.FromStream(ms);
                        pictureBox1.Image = img;
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            label1.Text = name; // Display product name
            textBox4.Text = quantity; // Display quantity
            label4.Text = price;
            textBox6.Text = Username;

            this.dataGridView1.CellClick += new DataGridViewCellEventHandler(dataGridView1_CellClick);
            this.dataGridView1.CellEndEdit += new DataGridViewCellEventHandler(dataGridView1_CellEndEdit);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            LoadCartData();
            CalculateTotalPrice();
        }

        private void LoadCartData()
        {
            int selectedRowIndex = -1;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                selectedRowIndex = dataGridView1.SelectedRows[0].Index;
            }

            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            string query = "SELECT name, category, price, quantity, photo FROM cart WHERE Username = @Username";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", Username);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;
            }

            if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows[selectedRowIndex].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = selectedRowIndex;
            }
            else if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.Rows[0].Selected = true; // Select the first row if no row was selected
            }

            UpdateTextBox4WithSelectedQuantity();
        }

        private void UpdateTextBox4WithSelectedQuantity()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var row = dataGridView1.SelectedRows[0];
                if (row.Cells["quantity"].Value != null)
                {
                    textBox4.Text = row.Cells["quantity"].Value.ToString(); // Set the text of textBox4 to quantity
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                if (row.Cells["name"].Value != null && row.Cells["quantity"].Value != null)
                {
                    string selectedName = row.Cells["name"].Value.ToString();
                    string selectedQuantity = row.Cells["quantity"].Value.ToString();

                    label1.Text = selectedName;
                    textBox4.Text = selectedQuantity; // Set textBox4 to quantity
                    label4.Text = row.Cells["price"].Value?.ToString();

                    byte[] selectedPhotoBytes = row.Cells["photo"].Value as byte[];
                    if (selectedPhotoBytes != null && selectedPhotoBytes.Length > 0)
                    {
                        using (var ms = new MemoryStream(selectedPhotoBytes))
                        {
                            try
                            {
                                Image img = Image.FromStream(ms);
                                pictureBox1.Image = img;
                                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("ไม่สามารถโหลดรูปภาพได้: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else
                    {
                        pictureBox1.Image = null; // Remove image if no data
                    }
                }
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "quantity")
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells["quantity"].Value != null && int.TryParse(row.Cells["quantity"].Value.ToString(), out int newQuantity))
                {
                    string itemName = row.Cells["name"].Value?.ToString();
                    if (!string.IsNullOrEmpty(itemName))
                    {
                        UpdateDatabaseQuantity(itemName, newQuantity);
                        textBox4.Text = newQuantity.ToString(); // Update textBox4 with the new quantity
                        CalculateTotalPrice(); // Recalculate total price
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox4.Text, out int quantity))
            {
                string itemName = label1.Text;

                if (quantity == 1)
                {
                    DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบรายการสินค้านี้?", "ยืนยัน", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        RemoveItemFromDatabase(itemName);
                        LoadCartData();
                        CalculateTotalPrice();
                    }
                }
                else if (quantity > 1)
                {
                    UpdateDatabaseQuantity(itemName, quantity - 1);
                    LoadCartData();
                    CalculateTotalPrice();
                }
            }
            else
            {
                MessageBox.Show("Quantity cannot be less than 1.");
            }
        }
        //ลบสิ้นค้าทีละ1
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox4.Text, out int newQuantity) && newQuantity >= 0)
            {
                string itemName = label1.Text;

                if (newQuantity == 0)
                {
                    DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบรายการสินค้านี้?", "ยืนยัน", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        RemoveItemFromDatabase(itemName);
                        LoadCartData();
                        CalculateTotalPrice();
                    }
                }
                else
                {
                    UpdateDatabaseQuantity(itemName, newQuantity);
                    CalculateTotalPrice();
                }
            }
            else
            {
                MessageBox.Show("กรุณากรอกจำนวนที่ถูกต้อง");
            }
        }

        //เพิ่มสินค้าทีละ1
        private void button2_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox4.Text, out int quantity))
            {
                string itemName = label1.Text;
                int availableStock = GetAvailableStock(itemName);

                if (quantity + 1 > availableStock)
                {
                    MessageBox.Show($"Cannot increase quantity beyond available stock. Available stock: {availableStock}", "Invalid Quantity");
                    return;
                }

                UpdateDatabaseQuantity(itemName, quantity + 1);
                LoadCartData();
                CalculateTotalPrice();
            }
            else
            {
                MessageBox.Show("Invalid quantity value.");
            }
        }

        //ลบสินค้า
        private void button3_Click(object sender, EventArgs e)
        {
            string itemName = label1.Text;
            if (!string.IsNullOrEmpty(itemName))
            {
                DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบรายการสินค้านี้?", "ยืนยัน", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        RemoveItemFromDatabase(itemName);
                        LoadCartData();
                        CalculateTotalPrice();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาดในการลบรายการสินค้า: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("ไม่ได้เลือกสินค้าที่จะลบ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //ยอดรวม
        //ยอดรวม
        private decimal totalPriceBeforeVAT; // Store total price before VAT globally

        private void CalculateTotalPrice()
        {
            decimal totalPrice = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["price"].Value != null &&
                    row.Cells["quantity"].Value != null &&
                    decimal.TryParse(row.Cells["price"].Value.ToString(), out decimal price) &&
                    int.TryParse(row.Cells["quantity"].Value.ToString(), out int quantity))
                {
                    totalPrice += price * quantity;
                }
            }

            totalPriceBeforeVAT = totalPrice; // Store total price before VAT
            decimal vatRate = 0.07m; // VAT rate of 7%
            decimal vatAmount = totalPrice * vatRate; // Calculate VAT amount
            decimal totalIncludingVAT = totalPrice + vatAmount; // Total amount including VAT

            label2.Text = vatAmount.ToString("C"); // Display VAT amount in label2
            label3.Text = totalIncludingVAT.ToString("C"); // Display total including VAT in label3
        }


        //อัพเดตฐานข้อมูล เมื่อเพิ่มสิ้นค้า
        private void UpdateDatabaseQuantity(string itemName, int newQuantity)
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            string query = "UPDATE cart SET quantity = @Quantity WHERE name = @Name AND Username = @Username";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Quantity", newQuantity);
                command.Parameters.AddWithValue("@Name", itemName);
                command.Parameters.AddWithValue("@Username", Username);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        //ลบสินค้าออก
        private void RemoveItemFromDatabase(string itemName)
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            string query = "DELETE FROM cart WHERE name = @Name AND Username = @Username";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", itemName);
                command.Parameters.AddWithValue("@Username", Username);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        //อัพเดตสินค้าในสต็อก
        private int GetAvailableStock(string itemName)
        {
            int availableStock = 0;
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            string query = "SELECT quantity FROM stock WHERE name = @Name"; // Adjust column name as needed

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Name", itemName);
                    connection.Open();

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        // Ensure the result is convertible to int
                        if (int.TryParse(result.ToString(), out int stock))
                        {
                            availableStock = stock;
                        }
                        else
                        {
                            MessageBox.Show("Error: The stock quantity is not in the expected format.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: Stock quantity not found or is null.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return availableStock;
        }

        //กลับหน้าเมนู


        private void button5_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            decimal totalIncludingVAT = decimal.Parse(label3.Text, System.Globalization.NumberStyles.Currency);
            Payment paymentForm = new Payment(Username, "", Category, name, price, textBox4.Text, photoBytes, totalIncludingVAT);
            paymentForm.Show();
        }




        private void button4_Click_1(object sender, EventArgs e)
        {
            menu menuForm = new menu(Username);
            menuForm.Show();
            this.Close();
        }
    }
}