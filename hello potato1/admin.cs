using hello_potato1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hello_potato1
{
    public partial class admin : Form
    {
        public admin()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            stock_management f1 = new stock_management();
            f1.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            User_management f1 = new User_management();
            f1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Total_summary f1 = new Total_summary();
            f1.Show();
        }

        private void admin_Load(object sender, EventArgs e)
        {

        }

     

        private void button4_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            login f1 = new login();
            f1.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Hide();
            stockedit1 f1 = new stockedit1();
            f1.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Hide();
            history f1 = new history();
            f1.Show();
        }
    }
}
