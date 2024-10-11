using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace hello_potato1
{
    public partial class UserEedit : Form
    {
        private string Username;
        private string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";

        public UserEedit(string username)
        {
            InitializeComponent();
            this.Username = username;
            LoadUserData();
            textBox1.Text = Username;
        }
        //โหลดข้อมูลมาแสดง
        private void LoadUserData()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM membership WHERE Username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", Username);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        textBox2.Text = reader["Name"].ToString();
                        textBox3.Text = reader["Email"].ToString();
                        textBox4.Text = reader["PhoneNumber"].ToString();
                        textBox5.Text = reader["Password"].ToString();
                    }
                }
            }
        }

        //ตรวจสอบรหัส
        private string PromptForPassword()
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Password Verification",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = "Enter your current password:" };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240, PasswordChar = '*' };
            Button confirmation = new Button() { Text = "OK", Left = 180, Width = 80, Top = 80, DialogResult = DialogResult.OK };

            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Validate user input
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อ.", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox2.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text) || !IsValidEmail(textBox3.Text))
            {
                MessageBox.Show("กรุณากรอกอีเมลที่ถูกต้อง.", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox3.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("กรุณากรอกหมายเลขโทรศัพท์.", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox4.Focus();
                return;
            }

            string newPassword = textBox5.Text;
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("กรุณากรอกรหัสผ่านใหม่.", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox5.Focus();
                return;
            }

            //บันทึกข้อมูล
            // Prompt for the current password
            string currentPassword = PromptForPassword();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string verifyQuery = "SELECT Password FROM membership WHERE Username = @Username";
                MySqlCommand verifyCmd = new MySqlCommand(verifyQuery, conn);
                verifyCmd.Parameters.AddWithValue("@Username", Username);

                string storedPassword = verifyCmd.ExecuteScalar()?.ToString();

                if (storedPassword == currentPassword)
                {
                    if (IsPasswordStrong(newPassword))
                    {
                        string updateQuery = "UPDATE membership SET Name = @Name, Email = @Email, PhoneNumber = @PhoneNumber, Password = @Password WHERE Username = @Username";
                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@Name", textBox2.Text);
                        updateCmd.Parameters.AddWithValue("@Email", textBox3.Text);
                        updateCmd.Parameters.AddWithValue("@PhoneNumber", textBox4.Text);
                        updateCmd.Parameters.AddWithValue("@Password", newPassword);
                        updateCmd.Parameters.AddWithValue("@Username", Username);

                        try
                        {
                            updateCmd.ExecuteNonQuery();
                            MessageBox.Show("ข้อมูลถูกอัปเดตเรียบร้อยแล้ว!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("เกิดข้อผิดพลาดในการอัปเดตข้อมูล: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("รหัสผ่านใหม่ไม่ตรงตามข้อกำหนด: อย่างน้อย 4 ตัวอักษร ต้องมีทั้งตัวอักษรและตัวเลข", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("รหัสผ่านปัจจุบันไม่ถูกต้อง", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


        private bool IsPasswordStrong(string password)
        {
            return password.Length >= 4 &&
                   password.Any(char.IsDigit) &&
                   password.Any(char.IsLetter) &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            menu menuForm = new menu(Username);
            menuForm.Show();
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e) { }
        private void UserEedit_Load(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void textBox5_TextChanged(object sender, EventArgs e) { }
    }
}
