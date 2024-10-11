using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace hello_potato1
{
    public partial class Payment : Form
    {
        private string Username;
        private string Category;
        private string id;
        private string name;
        private string price;
        private string quantity;
        private byte[] photoBytes;
        private decimal totalAmount;
        private string customerEmail;

        //ส่งยอด
        public Payment(string username, string category, string id, string name, string price, string quantity, byte[] photoBytes, decimal totalAmount)

        {
            InitializeComponent();
            this.Username = username;
            this.Category = category;
            this.id = id;
            this.name = name;
            this.price = price;
            this.quantity = quantity;
            this.photoBytes = photoBytes;
            this.totalAmount = totalAmount;

            // Display total amount
            label1.Text = $"{totalAmount:C}";

            // Generate and display QR code
            GenerateQRCode(totalAmount);

            // Fetch customer email
            customerEmail = GetCustomerEmail();
        }

        //เจนคิวอาร์
        private void GenerateQRCode(decimal amount)
        {
            string imageUrl = "https://promptpay.io/0960941078/" + totalAmount.ToString("F2") + ".png";
            pictureBox1.ImageLocation = imageUrl;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Complete payment
            CompletePayment();
        }

        private void CompletePayment()
        {
            // Print PDF receipt
            string filePath = PrintReceipt();

            // Store order details in the database
            StoreOrderDetails(filePath);

            // Update stock and clear cart
            UpdateStockAndClearCart();

            // Send receipt via email
            if (!string.IsNullOrEmpty(customerEmail))
            {
                SendReceiptEmail(filePath, customerEmail);
            }
            else
            {
                MessageBox.Show("Customer email not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Navigate to menu form
            NavigateToMenu();
        }
        //ใบเสร็จ
        private string PrintReceipt()
        {
            string folderPath = Path.Combine(Environment.CurrentDirectory, "HelloPDF");
            Directory.CreateDirectory(folderPath); // Ensure the folder exists

            string filePath = Path.Combine(folderPath, $"Receipt_{Username}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Document document = new Document(PageSize.B5, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("                 HELLO POTATO", FontFactory.GetFont("FC Igloo", 25)));
                document.Add(new Paragraph("              999 Sukhumvit Rd, Khwaeng Bang Na, Khet Bang Na, Bangkok 10270 "));
                document.Add(new Paragraph("                                    TAX ID : 099999994200 (VAT Included)"));
                document.Add(new Paragraph("                                                   Tel : 09609999920"));
                document.Add(new Paragraph("         -------------------------------------------------------------------------------------------------"));

                // Create Phrase for Customer information and add as a new paragraph
                Phrase customerPhrase = new Phrase();
                customerPhrase.Add(new Chunk("Customer: ", FontFactory.GetFont("FC Igloo", 13)));
                customerPhrase.Add(new Chunk($"ลูกค้า {Username}", FontFactory.GetFont("FC Igloo", 13)));
                document.Add(new Paragraph(customerPhrase));

                // Create Phrase for Date and add as a new paragraph
                Phrase datePhrase = new Phrase();
                datePhrase.Add(new Chunk("Date: ", FontFactory.GetFont("FC Igloo", 15)));
                datePhrase.Add(new Chunk($"วันที่ {DateTime.Now:dd MMM yyyy HH:mm:ss}", FontFactory.GetFont("FC Igloo", 13)));
                document.Add(new Paragraph(datePhrase));

                // Add Product List header with both English and Thai
                document.Add(new Paragraph("Product List", FontFactory.GetFont("FC Igloo", 13)));
                document.Add(new Paragraph("รายการที่ซื้อ", FontFactory.GetFont("FC Igloo", 13)));
                document.Add(new Paragraph("\n"));

                // Create a table for purchased items
                PdfPTable table = new PdfPTable(3); // 3 columns: Product Name, Price, Quantity
                table.WidthPercentage = 100; // Table width to be 100% of page width

                // Add table headers
                table.AddCell(CreateCell("Product Name", Element.ALIGN_CENTER));
                table.AddCell(CreateCell("Price", Element.ALIGN_CENTER));
                table.AddCell(CreateCell("Quantity", Element.ALIGN_CENTER));

                // Add purchased items to the table
                AddPurchasedItemsToTable(table);

                // Add the table to the document
                document.Add(table);

                // Calculate the amount before VAT and the VAT amount (assuming VAT is included in the total)
                decimal amountBeforeVAT = totalAmount / 1.07m;
                decimal vatAmount = totalAmount - amountBeforeVAT;

                // Add space before the VAT and Net Total
                document.Add(new Paragraph("\n"));

                // **Add Subtotal before VAT**
                Paragraph subtotalParagraph = new Paragraph($"Subtotal (Before VAT): {amountBeforeVAT:C} Baht", FontFactory.GetFont("FC Igloo", 13));
                subtotalParagraph.Alignment = Element.ALIGN_RIGHT; // Align to the right
                subtotalParagraph.SpacingBefore = 10; // Add some space before the subtotal
                document.Add(subtotalParagraph);

                // Add VAT amount
                Paragraph vatParagraph = new Paragraph($"VAT (7%): {vatAmount:C} Baht", FontFactory.GetFont("FC Igloo", 13));
                vatParagraph.Alignment = Element.ALIGN_RIGHT; // Align to the right
                vatParagraph.SpacingBefore = 10; // Add some space before the VAT
                document.Add(vatParagraph);

                // Add Net Total amount (which includes VAT)
                Paragraph totalParagraph = new Paragraph($"Net Total (Including VAT): {totalAmount:C} Baht", FontFactory.GetFont("FC Igloo", 13));
                totalParagraph.Alignment = Element.ALIGN_RIGHT; // Align to the right
                totalParagraph.SpacingBefore = 10; // Add some space before the total
                document.Add(totalParagraph);

                document.Add(new Paragraph("\n"));
                Paragraph thankYouParagraph1 = new Paragraph("Thank you for shopping with us", FontFactory.GetFont("FC Igloo", 18));
                thankYouParagraph1.Alignment = Element.ALIGN_CENTER; // Center align
                document.Add(thankYouParagraph1);

                Paragraph thankYouParagraph2 = new Paragraph("ขอบคุณที่ช็อปปิ้งกับเรา", FontFactory.GetFont("FC Igloo", 13));
                thankYouParagraph2.Alignment = Element.ALIGN_CENTER; // Center align
                document.Add(thankYouParagraph2);

                document.Close();
            }

            // Open PDF automatically
            System.Diagnostics.Process.Start(filePath);

            MessageBox.Show($"ใบเสร็จถูกบันทึกเป็น {filePath}", "การชำระเงินเสร็จสิ้น", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return filePath;
        }



        //ลบตาราง
        private PdfPCell CreateCell(string text, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, FontFactory.GetFont("FC Igloo", 13)));
            cell.HorizontalAlignment = alignment;
            cell.BorderWidth = 0; // Remove border
            return cell;
        }
          
        //เพิ่มข้อมูลลงออเดอร์
        private void AddPurchasedItemsToTable(PdfPTable table)
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            string query = "SELECT name, price, quantity FROM cart WHERE Username = @Username";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", Username);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string itemName = reader["name"].ToString();
                    string itemPrice = reader["price"].ToString();
                    string itemQuantity = reader["quantity"].ToString();

                    // Add cells to the table with centered text
                    table.AddCell(CreateCell(itemName, Element.ALIGN_CENTER));
                    table.AddCell(CreateCell(itemPrice, Element.ALIGN_CENTER));
                    table.AddCell(CreateCell(itemQuantity, Element.ALIGN_CENTER));
                }
            }
        }
        //บันทึกใบเสร็จ
        
        private void StoreOrderDetails(string filePath)
        {
            byte[] receiptBytes = File.ReadAllBytes(filePath); // อ่านไฟล์ PDF เป็น byte array

            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            string insertOrderQuery = "INSERT INTO userorder (id, Username, Product, category, quantity, CartID, date, total, status, Receipt) " +
                                      "VALUES (@id, @Username, @Product, @category, @quantity, @CartID, @date, @total, 'ชำระเงินแล้ว', @Receipt)";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    MySqlCommand command = new MySqlCommand(insertOrderQuery, connection);

                    // ดึงข้อมูลสินค้าทั้งหมดจากตะกร้าของลูกค้า
                    string productList = "";
                    string category = "";
                    int totalQuantity = 0;

                    string cartQuery = "SELECT name, category, quantity FROM cart WHERE Username = @Username";
                    MySqlCommand cartCommand = new MySqlCommand(cartQuery, connection);
                    cartCommand.Parameters.AddWithValue("@Username", Username);

                    connection.Open();

                    using (MySqlDataReader reader = cartCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // สะสมข้อมูลสินค้าทั้งหมดในรูปแบบ "สินค้า1 x จำนวน, สินค้า2 x จำนวน"
                            productList += reader["name"].ToString() + " x " + reader["quantity"].ToString() + ", ";
                            category = reader["category"].ToString(); // หมวดหมู่สินค้า
                            totalQuantity += Convert.ToInt32(reader["quantity"]); // นับจำนวนสินค้าทั้งหมด
                        }
                    }

                    // ตัดตัว ',' ที่เกินออกจาก productList ถ้ามี
                    if (productList.EndsWith(", "))
                    {
                        productList = productList.Substring(0, productList.Length - 2);
                    }

                    // กำหนดค่าให้กับคำสั่ง SQL
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@Username", Username);
                    command.Parameters.AddWithValue("@Product", productList);
                    command.Parameters.AddWithValue("@category", category);
                    command.Parameters.AddWithValue("@quantity", totalQuantity);
                    command.Parameters.AddWithValue("@CartID", id);
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                    command.Parameters.AddWithValue("@total", totalAmount); // ยอดรวมทั้งหมด
                    command.Parameters.AddWithValue("@Receipt", receiptBytes); // ใบเสร็จ PDF

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการบันทึกรายละเอียดการสั่งซื้อ: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void UpdateStockAndClearCart()
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Update stock quantity
                string updateStockQuery = "UPDATE stock s " +
                                          "JOIN cart c ON s.id = c.id " +
                                          "SET s.quantity = s.quantity - c.quantity " +
                                          "WHERE c.Username = @Username";

                MySqlCommand updateStockCommand = new MySqlCommand(updateStockQuery, connection);
                updateStockCommand.Parameters.AddWithValue("@Username", Username);
                updateStockCommand.ExecuteNonQuery();

                // Clear cart
                string clearCartQuery = "DELETE FROM cart WHERE Username = @Username";
                MySqlCommand clearCartCommand = new MySqlCommand(clearCartQuery, connection);
                clearCartCommand.Parameters.AddWithValue("@Username", Username);
                clearCartCommand.ExecuteNonQuery();
            }
        }

        //ส่งเมลให้ลูกค้าดึงข้อมูลจากฐานข้อมูล
        private string GetCustomerEmail()
        {
            string email = null;
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=hellopotato;";
            string query = "SELECT Email FROM membership WHERE Username = @Username";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", Username);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    email = reader["Email"].ToString();
                }
            }

            return email;
        }
        //ที่อยู่เมล  //รูปแบบเมล
        private void SendReceiptEmail(string filePath, string customerEmail)
        {
            string fromEmail = "premiumvanbooking@gmail.com"; // Your email address
            string fromPassword = "qabl rskv gjpp nojs"; // Your app-specific password

            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587); // Gmail SMTP server and port

            try
            {
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(customerEmail); // Customer's email address
                mail.Subject = "Your Payment Receipt";
                mail.Body = $"Dear {Username},\n\nThank you for your purchase. Please find your receipt attached.\n\nBest regards,\nHello Potato";

                Attachment attachment = new Attachment(filePath);
                mail.Attachments.Add(attachment);

                smtpClient.Credentials = new NetworkCredential(fromEmail, fromPassword);
                smtpClient.EnableSsl = true; // Enable SSL

                smtpClient.Send(mail);

                MessageBox.Show("ใบเสร็จถูกส่งไปยังอีเมลของลูกค้าเรียบร้อยแล้ว", "การชำระเงินเสร็จสิ้น", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending the email: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavigateToMenu()
        {
            menu menuForm = new menu(Username);
            menuForm.Show();
            this.Close();
        }

        private void Payment_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Prompt user for confirmation to cancel payment
            DialogResult result = MessageBox.Show("คุณต้องการยกเลิกการจ่ายเงินหรือไม่?", "ยืนยันการยกเลิก", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Navigate back to Form3 without changing the cart
                Form3 form3 = new Form3(id, Username, name, Category, price, quantity, photoBytes); // Pass necessary parameters
                form3.Show();
                this.Close(); // Close the current payment form
            }
        }

    }
}