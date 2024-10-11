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
        private string description; // Field to hold the description

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

            textBox8.Text = Username; // Display the Username in textBox8
            LoadDetails();
        }

        private void LoadDetails()
        {
            textBox1.Text = id;
            textBox2.Text = name;
            textBox3.Text = price;
            textBox4.Text = quantity; // Assuming this is the stock quantity, set it to another variable if needed

            // Initialize textBox5 with default quantity
            textBox5.Text = "1";

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

                textBox7.Text = description; // Display the description in textBox7
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
                textBox6.Text = (result != DBNull.Value) ? result.ToString() : "0";
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox5.Text, out int newQuantity) && newQuantity > 0)
            {
                quantity = newQuantity.ToString();
            }
            else
            {
                textBox5.Text = "1"; // คืนค่าเริ่มต้นเป็น 1 หากข้อมูลไม่ถูกต้อง
                MessageBox.Show("Please enter a valid quantity.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox5.Text, out int orderQuantity) && orderQuantity > 0)
            {
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



        private void button6_Click(object sender, EventArgs e)
        {
            // Pass necessary data when opening Form3
            Form3 f1 = new Form3(id, Username, name, Category, price, textBox5.Text, photoBytes);
            f1.Show();
            this.Hide(); // Hide current form
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox5.Text, out int amount))
            {
                amount++;
                textBox5.Text = amount.ToString();
            }
            else
            {
                MessageBox.Show("Invalid amount value.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox5.Text, out int amount))
            {
                if (amount > 0)
                {
                    amount--;
                    textBox5.Text = amount.ToString();
                }
            }
            else
            {
                MessageBox.Show("Invalid amount value.");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void textBox6_TextChanged(object sender, EventArgs e) { }

        private void textBox8_TextChanged(object sender, EventArgs e) { }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            menu menu = new menu(Username);
            menu.Show();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Pass necessary data when opening Form3
            Form3 f1 = new Form3(id, Username, name, Category, price, textBox5.Text, photoBytes);
            f1.Show();
            this.Hide(); // Hide current form
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
