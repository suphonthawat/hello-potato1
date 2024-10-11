using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hello_potato1
{
    public partial class Form2 : Form
    {
        private string id;
        private string name;
        private string price;
        private string quantity;
        private string Username;
        private string Category;
        private byte[] photoBytes;
        private string description;

        private string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato";

        public Form2(string Username, string Category, string id, string name, string price, string quantity, byte[] photoBytes)
        {
            InitializeComponent();
            this.Username = Username;
            this.Category = Category;
            this.id = id;
            this.name = name;
            this.price = price;
            this.quantity = quantity;
            this.photoBytes = photoBytes;

            LoadDetails();
        }

        private void LoadDetails()
        {
            label1.Text = Username; // Display the Username in label1
            label2.Text = id;
            label3.Text = name;
            label4.Text = price;
            label5.Text = quantity;

            // Initialize textBox2 with default quantity
            textBox2.Text = "1";

            // Fetch description from the database
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Query to get the description from the stock table
                string query = "SELECT description FROM stock WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                object result = cmd.ExecuteScalar();
                description = result != DBNull.Value ? result.ToString() : "No description available";

                textBox1.Text = description; // Display the description in textBox1
            }

            if (photoBytes != null && photoBytes.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(photoBytes))
                {
                    Image image = Image.FromStream(ms);
                    pictureBox1.Image = image;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }

            UpdateTotalQuantity();
        }

        //ตรวจจำนวนสินค้าว่ามีเท่าไหร่
        private void UpdateTotalQuantity()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Sum up the quantity of all items in the cart for the current Username
                string query = "SELECT SUM(quantity) FROM cart WHERE Username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", Username);

                object result = cmd.ExecuteScalar();

                // Check if the result is not null and convert it to string
                label6.Text = (result != DBNull.Value) ? result.ToString() : "0"; // Display total quantity in label6
            }
        }

        //เก็บข้อมูลสินค้า
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox2.Text, out int newQuantity) && newQuantity > 0)
            {
                int availableStock = GetAvailableStock();

                if (newQuantity > availableStock)
                {
                    MessageBox.Show($"The quantity entered exceeds the available stock. Available stock: {availableStock}", "Invalid Quantity");
                    textBox2.Text = availableStock.ToString();
                    quantity = availableStock.ToString();
                }
                else
                {
                    quantity = newQuantity.ToString();
                }
            }
            else
            {
                textBox2.Text = "1"; // Set default value to 1 if input is invalid
                MessageBox.Show("Please enter a valid quantity.");
            }
        }

        private void button3_Click(object sender, EventArgs e) // Add to cart
        {
            if (int.TryParse(textBox2.Text, out int orderQuantity) && orderQuantity > 0)
            {
                int availableStock = GetAvailableStock();

                if (orderQuantity > availableStock)
                {
                    MessageBox.Show($"Cannot add more than available stock. Available stock: {availableStock}", "Invalid Quantity");
                    textBox2.Text = availableStock.ToString();
                    return;
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if the item already exists in the cart
                    MySqlCommand checkCartCmd = new MySqlCommand("SELECT quantity FROM cart WHERE id = @id AND Username = @Username", conn);
                    checkCartCmd.Parameters.AddWithValue("@id", id);
                    checkCartCmd.Parameters.AddWithValue("@Username", Username);
                    object cartQuantityObj = checkCartCmd.ExecuteScalar();

                    if (cartQuantityObj != null)
                    {
                        // If it exists, update the quantity
                        int currentCartQuantity = int.Parse(cartQuantityObj.ToString());

                        if (currentCartQuantity + orderQuantity > availableStock)
                        {
                            MessageBox.Show($"Total quantity in cart exceeds available stock. Available stock: {availableStock}", "Invalid Quantity");
                            return;
                        }

                        MySqlCommand updateCartCmd = new MySqlCommand("UPDATE cart SET quantity = @quantity WHERE id = @id AND Username = @Username", conn);
                        updateCartCmd.Parameters.AddWithValue("@quantity", currentCartQuantity + orderQuantity);
                        updateCartCmd.Parameters.AddWithValue("@id", id);
                        updateCartCmd.Parameters.AddWithValue("@Username", Username);
                        updateCartCmd.ExecuteNonQuery();
                        MessageBox.Show("Updated quantity in cart!", "Updated");
                    }
                    else
                    {
                        // If not in the cart, add new item
                        MySqlCommand selectStockCmd = new MySqlCommand("SELECT name, category, price, photo FROM stock WHERE id = @id", conn);
                        selectStockCmd.Parameters.AddWithValue("@id", id);
                        MySqlDataReader reader = selectStockCmd.ExecuteReader();

                        string name = "";
                        string category = "";
                        decimal price = 0;
                        byte[] photoBytes = null;

                        if (reader.Read())
                        {
                            name = reader.GetString(reader.GetOrdinal("name"));
                            category = reader.GetString(reader.GetOrdinal("category"));
                            price = reader.GetDecimal(reader.GetOrdinal("price"));
                            if (!reader.IsDBNull(reader.GetOrdinal("photo")))
                            {
                                photoBytes = (byte[])reader["photo"];
                            }
                        }
                        reader.Close();

                        MySqlCommand insertCartCmd = new MySqlCommand("INSERT INTO cart (id, Username, name, category, price, quantity, photo) VALUES (@id, @Username, @name, @category, @price, @quantity, @photo)", conn);
                        insertCartCmd.Parameters.AddWithValue("@id", id);
                        insertCartCmd.Parameters.AddWithValue("@Username", Username);
                        insertCartCmd.Parameters.AddWithValue("@name", name);
                        insertCartCmd.Parameters.AddWithValue("@category", category);
                        insertCartCmd.Parameters.AddWithValue("@price", price);

                        if (photoBytes != null)
                        {
                            insertCartCmd.Parameters.AddWithValue("@photo", photoBytes);
                        }
                        else
                        {
                            insertCartCmd.Parameters.AddWithValue("@photo", DBNull.Value);
                        }

                        insertCartCmd.Parameters.AddWithValue("@quantity", orderQuantity);
                        try
                        {
                            insertCartCmd.ExecuteNonQuery();
                            MessageBox.Show("Added to cart successfully!", "Success");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message, "Error");
                        }
                    }

                    // Update total quantity in cart
                    UpdateTotalQuantity();
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid quantity.", "Error");
            }
        }

        private void button5_Click(object sender, EventArgs e) // Increase quantity by 1
        {
            if (int.TryParse(textBox2.Text, out int amount))
            {
                int availableStock = GetAvailableStock();

                if (amount < availableStock)
                {
                    amount++;
                    textBox2.Text = amount.ToString();
                }
                else
                {
                    MessageBox.Show($"Cannot exceed available stock. Available stock: {availableStock}", "Invalid Quantity");
                }
            }
            else
            {
                MessageBox.Show("Invalid amount value.");
            }
        }

        private void button4_Click(object sender, EventArgs e) // Decrease quantity by 1
        {
            if (int.TryParse(textBox2.Text, out int amount))
            {
                if (amount > 1)
                {
                    amount--;
                    textBox2.Text = amount.ToString();
                }
            }
            else
            {
                MessageBox.Show("Invalid amount value.");
            }
        }

        private void button2_Click(object sender, EventArgs e) // Back to Menu
        {
            this.Hide();
            menu menu = new menu(Username);
            menu.Show();
        }

        private void button1_Click(object sender, EventArgs e) // Go to next
        {
            // Logic to navigate to the payment form
            this.Hide();
            decimal totalAmount = CalculateTotalAmount(); // Calculate total amount based on items in the cart
            Payment paymentForm = new Payment(Username, Category, id, name, price, quantity, photoBytes, totalAmount);
            paymentForm.Show();
        }

        private int GetAvailableStock()
        {
            int availableStock = 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Query to get the available stock for the current item
                string query = "SELECT quantity FROM stock WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                object result = cmd.ExecuteScalar();

                if (result != DBNull.Value)
                {
                    availableStock = Convert.ToInt32(result);
                }
            }

            return availableStock;
        }

        //ข้อมูลสินค้าในตะกร้า
        private decimal CalculateTotalAmount()
        {
            decimal totalAmount = 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Query to sum up the total amount in the cart
                string query = "SELECT SUM(price * quantity) FROM cart WHERE Username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", Username);

                object result = cmd.ExecuteScalar();

                if (result != DBNull.Value)
                {
                    totalAmount = Convert.ToDecimal(result);
                }
            }

            return totalAmount;
        }


       //ไปหน้ารวมตะกร้า
        private void button6_Click(object sender, EventArgs e)
        {
            // Create an instance of Form3 with the required parameters
            Form3 form3Instance = new Form3(id, Username, name, Category, price, quantity, photoBytes);
            form3Instance.Show(); // Show Form3
            this.Hide(); // Hide Form2
        }

        //กลับหน้าเมนู
        private void button2_Click_1(object sender, EventArgs e)
        {
            // Create an instance of the menu form
            menu menuForm = new menu(Username); // Pass Username to the menu form
            menuForm.Show(); // Show the menu form
            this.Hide(); // Hide Form2
        }

        //ไปหน้าจ่ายเงิน
        private void button1_Click_1(object sender, EventArgs e)
        {
            // Calculate the total amount based on items in the cart
            decimal totalAmount = CalculateTotalAmount();
            // Create an instance of the Payment form with the required parameters
            Payment paymentForm = new Payment(Username, Category, id, name, price, quantity, photoBytes, totalAmount);
            paymentForm.Show(); // Show the Payment form
            this.Hide(); // Hide Form2
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}