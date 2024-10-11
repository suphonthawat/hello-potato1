using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace hello_potato1
{
    public partial class Total_summary : Form
    {
        private string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";

        public Total_summary()
        {
            InitializeComponent();
            comboBox1.Items.Add("Daily Sales");
            comboBox1.Items.Add("Monthly Sales");
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox1.SelectedIndex = 0; // Default to Daily Sales
        }

        private void Total_summary_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Today;

            UpdateLabels();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            admin f1 = new admin();
            f1.Show();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            UpdateLabels();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            UpdateLabels();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLabels();
            ToggleDatePickerAndLabel();
        }

        private void UpdateLabels()
        {
            DateTime selectedDate1 = dateTimePicker1.Value.Date;
            DateTime selectedDate2 = dateTimePicker2.Value.Date;

            label3.Text = $"วันที่เริ่มต้น: {selectedDate1.ToShortDateString()}";

            decimal totalSales = 0;

            if (comboBox1.SelectedItem.ToString() == "Daily Sales")
            {
                totalSales = GetDailySalesTotalByDate(selectedDate1);
                label1.Text = $"ยอดขายรายวัน: {totalSales:C} บาท";
                label4.Text = ""; // Hide label4 text for daily sales

                // Update DataGridView with daily sales data
                BindDataToGridView(selectedDate1, selectedDate1);
            }
            else if (comboBox1.SelectedItem.ToString() == "Monthly Sales")
            {
                totalSales = GetSalesTotalForDateRange(selectedDate1, selectedDate2);
                label1.Text = $"ยอดขายรายเดือน: {totalSales:C} บาท";
                label4.Text = $"วันที่สิ้นสุด: {selectedDate2.ToShortDateString()}"; // Show label4 text for monthly sales

                // Update DataGridView with monthly sales data
                BindDataToGridView(selectedDate1, selectedDate2);
            }
        }

        private void ToggleDatePickerAndLabel()
        {
            if (comboBox1.SelectedItem.ToString() == "Daily Sales")
            {
                dateTimePicker2.Enabled = false;
                label4.Visible = false;
            }
            else if (comboBox1.SelectedItem.ToString() == "Monthly Sales")
            {
                dateTimePicker2.Enabled = true;
                label4.Visible = true;
            }
        }

        // Method to bind data to the DataGridView based on date range
        private void BindDataToGridView(DateTime startDate, DateTime endDate)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT Username, Product, quantity, date, total FROM `userorder` WHERE DATE(date) BETWEEN @StartDate AND @EndDate";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    System.Data.DataTable dataTable = new System.Data.DataTable();
                    adapter.Fill(dataTable);

                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"เกิดข้อผิดพลาดในการดึงข้อมูล: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private decimal GetDailySalesTotalByDate(DateTime date)
        {
            decimal totalSales = 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT SUM(total) FROM `userorder` WHERE DATE(date) = @OrderDate";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@OrderDate", date.ToString("yyyy-MM-dd"));

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        totalSales = Convert.ToDecimal(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"เกิดข้อผิดพลาดในการดึงยอดขายรายวัน: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return totalSales;
        }

        private decimal GetSalesTotalForDateRange(DateTime startDate, DateTime endDate)
        {
            decimal totalSales = 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT SUM(total) FROM `userorder` WHERE DATE(date) BETWEEN @StartDate AND @EndDate";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        totalSales = Convert.ToDecimal(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"เกิดข้อผิดพลาดในการดึงยอดขายรายเดือน: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return totalSales;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

       

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        
    }
}
