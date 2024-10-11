using System;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hello_potato1
{
    public partial class Newpassword : Form
    {
        private string email;
        private string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";

        public Newpassword(string userEmail)
        {
            InitializeComponent();
            email = userEmail;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string newPassword = textBox1.Text.Trim();

            if (newPassword.Length >= 4 && IsPasswordStrong(newPassword))
            {
                UpdatePassword(email, newPassword);
                MessageBox.Show("รหัสผ่านของคุณได้รับการอัปเดตเรียบร้อยแล้ว. โปรดเข้าสู่ระบบด้วยรหัสผ่านใหม่", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close(); // ปิดฟอร์มหลังจากอัพเดตรหัสผ่าน

                // Open the login form
                login loginForm = new login();
                loginForm.Show();
            }
            else
            {
                MessageBox.Show("รหัสผ่านใหม่ไม่ตรงตามเงื่อนไข", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePassword(string email, string newPassword)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE membership SET password = @Password WHERE email = @Email";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Password", newPassword);
                cmd.Parameters.AddWithValue("@Email", email);

                cmd.ExecuteNonQuery();
            }
        }

        private bool IsPasswordStrong(string password)
        {
            // Check for password strength criteria (you can add more conditions as needed)
            return password.Any(char.IsDigit) && password.Any(char.IsLetter);
        }

        private void Newpassword_Load(object sender, EventArgs e)
        {

        }
    }
}
