using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace hello_potato1
{
    public partial class menu : Form
    {
        private string Username;
        private string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";

        public menu(string username)
        {
            InitializeComponent();
            this.Username = username;
            LoadImages();
            textBox1.Text = Username;

            // Update total quantity when form loads
            UpdateTotalQuantity();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
        }

        private MySqlConnection databaseConnection()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        //ถ้าสินค้ามีค่า0 จะไม่แสดง
        private void LoadImages()
        {
            MySqlConnection conn = databaseConnection();
            try
            {
                conn.Open();
                // Updated query to exclude products with quantity = 0
                string query = "SELECT id, name, price, quantity, photo FROM stock WHERE category = 'menu' AND quantity > 0 ORDER BY price ASC";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                flowLayoutPanel1.Controls.Clear(); // Clear existing controls

                while (reader.Read())
                {
                    string id = reader["id"].ToString();
                    string name = reader["name"].ToString();
                    string price = reader["price"].ToString();
                    string quantity = reader["quantity"].ToString();
                    byte[] photoBytes = reader["photo"] as byte[];

                    if (photoBytes != null && photoBytes.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(photoBytes))
                        {
                            Image image = Image.FromStream(ms);

                            // Create a panel to hold all data
                            Panel panel = new Panel
                            {
                                Width = 170,
                                Height = 220,
                                BorderStyle = BorderStyle.None
                            };

                            // Create PictureBox to display image
                            PictureBox pictureBox = new PictureBox
                            {
                                Image = image,
                                SizeMode = PictureBoxSizeMode.StretchImage,  // Change to StretchImage to fit the PictureBox
                                Dock = DockStyle.Top,
                                Width = 170,  // Set the width of PictureBox to match the panel's width
                                Height = 220  // Set height of PictureBox
                            };

                            // Set click event for PictureBox
                            pictureBox.Click += (s, e) =>
                            {
                                this.Hide();
                                Form2 form2 = new Form2(Username, "", id, name, price, quantity, photoBytes); // Updated constructor call
                                form2.Show();
                            };

                            // Add PictureBox to Panel
                            panel.Controls.Add(pictureBox);

                            // Add Panel to FlowLayoutPanel
                            flowLayoutPanel1.Controls.Add(panel);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No photo data found for this record.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        //เพิ่มสินค้าไปเก็บในตะกร้า
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

        private void flowLayoutPanel1_Paint_1(object sender, PaintEventArgs e)
        {
        }

        //ปุ่มกลับ
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("คุณแน่ใจหรือว่าต้องการ Log out?", "ยืนยันเพื่อออก", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Hide();
                login f1 = new login();
                f1.Show(); 
            }
        }

        //ไปหน้าตะกร้า
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("คุณต้องการไปหน้าตะกร้าหรือไม่?", "ยืนยันเพื่อดำเนินการต่อ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {   // Pass necessary data when opening Form3
                string id = ""; // Set appropriate id if needed
                string name = ""; // Set appropriate name if needed
                string price = ""; // Set appropriate price if needed
                string quantity = textBox6.Text; // Use the total quantity in the cart
                byte[] photoBytes = null; // Set appropriate photo if needed

                Form3 form3 = new Form3(id, Username, name, Category: "menu", price, quantity, photoBytes);
                form3.Show();
                this.Hide();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }
        //ไปรถเข็นอีกอัน
        private void button6_Click(object sender, EventArgs e)
        {
            // ไม่ต้องการการแจ้งเตือนเมื่อไปยัง Form3
            string id = ""; // กำหนด id ที่เหมาะสมถ้าจำเป็น
            string name = ""; // กำหนด name ที่เหมาะสมถ้าจำเป็น
            string price = ""; // กำหนด price ที่เหมาะสมถ้าจำเป็น
            string quantity = textBox6.Text; // ใช้จำนวนรวมในตะกร้า
            byte[] photoBytes = null; // กำหนด photo ที่เหมาะสมถ้าจำเป็น

            Form3 form3 = new Form3(id, Username, name, Category: "menu", price, quantity, photoBytes);
            form3.Show();
            this.Hide();
        }


        private void textBox6_TextChanged(object sender, EventArgs e)
        {
        }
        //ไปหน้าเมนูน้ำ
        private void button4_Click(object sender, EventArgs e)
        {
            // Create an instance of Form1 with the necessary parameters
            Drink form1 = new Drink(Username);

            // Show Form1
            form1.Show();

            // Hide the current menu form
            this.Hide();
        }
        
        //ไปหน้าข้อมูลลูกค้า
        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            UserEedit f1 = new UserEedit(Username);
            f1.Show();

        }
    }
}
 