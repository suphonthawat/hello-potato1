using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace hello_potato1
{
    public partial class User_management : Form
    {
        string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";

        public User_management()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            admin f1 = new admin();
            f1.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SearchUser();
        }

        private void SearchUser()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT phonenumber, name, username FROM membership WHERE phonenumber = @phonenumber";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@phonenumber", textBox2.Text);

                try
                {
                    conn.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    if (table.Rows.Count > 0)
                    {
                        // Display user details in text boxes
                        DataRow row = table.Rows[0];
                        textBox1.Text = row["name"].ToString();
                        textBox3.Text = row["username"].ToString();

                        // Load the data into the DataGridView
                        dataGridView1.DataSource = table;
                    }
                    else
                    {
                        MessageBox.Show("User not found.");
                        // Clear the text boxes and DataGridView if no user is found
                        textBox1.Clear();
                        textBox3.Clear();
                        dataGridView1.DataSource = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateUser();
        }

        private void UpdateUser()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "UPDATE membership SET name = @name, username = @username WHERE phonenumber = @phonenumber";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", textBox1.Text);
                cmd.Parameters.AddWithValue("@username", textBox3.Text);
                cmd.Parameters.AddWithValue("@phonenumber", textBox2.Text);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("User updated successfully!");
                        SearchUser(); // Refresh DataGridView with updated user info
                    }
                    else
                    {
                        MessageBox.Show("Update failed.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void User_management_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Check if the user has entered a phone number to delete
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please enter a phone number to delete.");
                return;
            }

            // Confirm deletion
            DialogResult result = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DeleteUser();
            }
        }

        private void DeleteUser()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "DELETE FROM membership WHERE phonenumber = @phonenumber";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@phonenumber", textBox2.Text);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("User deleted successfully!");

                        // Clear the text boxes and DataGridView after deletion
                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();
                        dataGridView1.DataSource = null;
                    }
                    else
                    {
                        MessageBox.Show("Delete failed. User not found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

    }
}
