using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace hello_potato1
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();

            txtPass.PasswordChar = '*'; // ตั้งค่า PasswordChar เพื่อไม่ให้แสดงรหัสผ่านเป็นตัวอักษร
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtUser.Text == "admin" && txtPass.Text == "1234")
            {
                MessageBox.Show("Welcome Admin", "Message");
                this.Hide();
                admin f1 = new admin(); // เปิดหน้าต่าง admin
                f1.Show();
            }
            else if (txtUser.Text == "" && txtPass.Text == "")
            {
                MessageBox.Show("กรุณากรอกชื่อผู้ใช้และรหัสผ่านให้ถูกต้อง", "Message");
                txtUser.Focus();
            }
            else if (txtUser.Text == "")
            {
                MessageBox.Show("กรุณากรอกชื่อผู้ใช้", "Message");
                txtUser.Focus();
            }
            else if (txtPass.Text == "")
            {
                MessageBox.Show("กรุณากรอกรหัสผ่าน", "Message");
                txtPass.Focus();
            }
            else
            {
                string sql = "SELECT * FROM membership WHERE Username = @username AND Password = @password";
                using (MySqlConnection conn = new MySqlConnection("datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;"))
                {
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@username", txtUser.Text);
                    cmd.Parameters.AddWithValue("@password", txtPass.Text);

                    try
                    {
                        conn.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            string Username = txtUser.Text; // ดึงค่า username จาก TextBox
                            MessageBox.Show($"Hello {Username} 🙂", "Message"); // แสดงข้อความ Hello พร้อมกับ Username
                            this.Hide();
                            menu f1 = new menu(Username); // ส่งค่า username ไปยังฟอร์ม menu
                            f1.Show();
                        }
                        else
                        {
                            MessageBox.Show("กรุณากรอกชื่อผู้ใช้และรหัสผ่านให้ถูกต้อง", "Message");
                            txtUser.Focus();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error");
                    }
                }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            member f1 = new member();
            f1.Show();
        }

       

        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            txtPass.PasswordChar = '\0'; // แสดงรหัสผ่านเมื่อกดปุ่ม
        }

        private void button4_MouseUp(object sender, MouseEventArgs e)
        {
            txtPass.PasswordChar = '*'; // ซ่อนรหัสผ่านเมื่อปล่อยปุ่ม
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void login_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Hide();
            Contact_address f1 = new Contact_address();
            f1.Show();
        }

        // ฟังก์ชันแสดงรหัสผ่านเมื่อกดค้างปุ่ม button6
        private void button6_MouseDown(object sender, MouseEventArgs e)
        {
            txtPass.PasswordChar = '\0'; // แสดงรหัสผ่านเมื่อกดปุ่ม
        }

        private void button6_MouseUp(object sender, MouseEventArgs e)
        {
            txtPass.PasswordChar = '*'; // ซ่อนรหัสผ่านเมื่อปล่อยปุ่ม
        }
    }
}
