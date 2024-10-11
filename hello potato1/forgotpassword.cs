using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace hello_potato1
{
    public partial class forgotpassword : Form
    {
        private string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
        private string generatedOtp;
        private DateTime otpExpirationTime;
        private Timer countdownTimer;
        private int remainingSeconds;

        public forgotpassword()
        {
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            countdownTimer = new Timer();
            countdownTimer.Interval = 1000; // Set timer interval to 1 second
            countdownTimer.Tick += CountdownTimer_Tick;
        }

        //ส่งOTP 
        private async void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text.Trim();

            if (IsEmailExists(email))
            {
                generatedOtp = GenerateOtp();
                otpExpirationTime = DateTime.Now.AddMinutes(1).AddSeconds(30); // Set OTP expiration time

                await SendOtpEmail(email, generatedOtp);

                // Start countdown timer
                remainingSeconds = 90; // 1 minute and 30 seconds
                countdownTimer.Start();
                button1.Enabled = false; // Disable the button while timer is running
            }
            else
            {
                MessageBox.Show("อีเมลนี้ไม่มีอยู่ในระบบ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                labelCountdown.Text = $"จะส่งรหัสได้อีกใน {remainingSeconds} "; // Display remaining time
            }
            else
            {
                countdownTimer.Stop();
                labelCountdown.Text = string.Empty; // Clear countdown display
                button1.Enabled = true; // Enable the button once countdown is complete
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string enteredOtp = textBox2.Text.Trim();

            if (DateTime.Now > otpExpirationTime)
            {
                MessageBox.Show("OTP หมดอายุ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (enteredOtp == generatedOtp)
            {
                // OTP ถูกต้อง, เปิดหน้าสำหรับเปลี่ยนรหัสผ่านใหม่
                Newpassword newPasswordForm = new Newpassword(textBox1.Text.Trim());
                newPasswordForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("OTP ไม่ถูกต้อง", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsEmailExists(string email)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM membership WHERE email = @Email";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }

        private async Task SendOtpEmail(string email, string otp)
        {
            try
            {
                var fromAddress = new MailAddress("premiumvanbooking@gmail.com", "Hello Potato");
                var toAddress = new MailAddress(email);
                const string fromPassword = "qabl rskv gjpp nojs"; // ใช้รหัสผ่านของแอปที่สร้างขึ้น
                const string subject = "OTP VERIFY";
                string body = $"Your OTP code is: {otp}";

                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential(fromAddress.Address, fromPassword);
                    smtpClient.EnableSsl = true;

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                }
                MessageBox.Show($"OTP ได้ถูกส่งไปยังอีเมลของคุณ ({email})");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send OTP email: " + ex.Message);
            }
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString(); // Generate 4-digit OTP
        }

        private void forgotpassword_Load(object sender, EventArgs e)
        {
            // You can initialize form components here if needed.
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            login f1 = new login();
            f1.Show();
        }
    }
}
