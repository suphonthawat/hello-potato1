using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hello_potato1
{
    public partial class member : Form
    {
        public member()
        {
            InitializeComponent();
        }

        private MySqlConnection databaseConnection()
        {
            // Ensure the database name is correct
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        private bool IsEmailValid(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.(com|co\.th)$";
            return Regex.IsMatch(email, pattern);
        }

        private bool IsPhoneNumberValid(string phoneNumber)
        {
            string pattern = @"^\d{10}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        private bool IsUserExists(string email, string phoneNumber, string username)
        {
            bool exists = false;
            using (MySqlConnection conn = databaseConnection())
            {
                string query = "SELECT COUNT(*) FROM membership WHERE Email = @Email OR Phonenumber = @Phonenumber OR Username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Phonenumber", phoneNumber);
                cmd.Parameters.AddWithValue("@Username", username);

                try
                {
                    conn.Open();
                    exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error");
                }
            }
            return exists;
        }

        private bool IsPasswordSecure(string password)
        {
            // อัปเดต pattern ให้รองรับอักขระพิเศษทุกชนิด
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s])[A-Za-z\d\W]{6,}$";
            return Regex.IsMatch(password, pattern);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox4.Text) ||
                string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("กรุณากรอกข้อมูลให้ครบทุกช่อง", "Message");
                return;
            }

            if (!IsEmailValid(textBox2.Text))
            {
                MessageBox.Show("อีเมลไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง", "Error");
                return;
            }

            if (!IsPhoneNumberValid(textBox4.Text))
            {
                MessageBox.Show("หมายเลขโทรศัพท์ต้องเป็นตัวเลข 10 หลัก", "Error");
                return;
            }

            if (textBox3.Text.Length < 4)
            {
                MessageBox.Show("ชื่อผู้ใช้ต้องมีความยาวอย่างน้อย 4 ตัว", "Error");
                return;
            }

            if (textBox5.Text.Length < 4)
            {
                MessageBox.Show("รหัสผ่านต้องมีความยาวอย่างน้อย 4 ตัว", "Error");
                return;
            }

            if (!IsPasswordSecure(textBox5.Text))
            {
                MessageBox.Show("รหัสผ่านต้องมีความปลอดภัย (ประกอบด้วยตัวอักษรใหญ่ ตัวอักษรเล็ก ตัวเลข และอักขระพิเศษ)", "Error");
                return;
            }

            if (IsUserExists(textBox2.Text, textBox4.Text, textBox3.Text))
            {
                MessageBox.Show("อีเมล, หมายเลขโทรศัพท์ หรือชื่อผู้ใช้นี้ได้ถูกใช้ไปแล้ว โปรดเข้าสู่ระบบ", "Error");
                this.Hide();
                login f1 = new login();
                f1.Show();
                return;
            }

            using (MySqlConnection con = databaseConnection())
            {
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "INSERT INTO membership (Name, Email, Username, Password, Phonenumber) VALUES (@Name, @Email, @Username, @Password, @Phonenumber)";
                cmd.Parameters.AddWithValue("@Name", textBox1.Text);
                cmd.Parameters.AddWithValue("@Email", textBox2.Text);
                cmd.Parameters.AddWithValue("@Username", textBox3.Text);
                cmd.Parameters.AddWithValue("@Password", textBox5.Text);
                cmd.Parameters.AddWithValue("@Phonenumber", textBox4.Text);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("สมัครผู้ใช้สำเร็จ", "Message");
                        this.Hide();
                        login f1 = new login();
                        f1.Show();
                    }
                    else
                    {
                        MessageBox.Show("เกิดข้อผิดพลาดในการสมัครผู้ใช้", "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error");
                }
            }
        }

        private void member_Load_1(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            login f1 = new login();
            f1.Show();
        }

      
    }
}
