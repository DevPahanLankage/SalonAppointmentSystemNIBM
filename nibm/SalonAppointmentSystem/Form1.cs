using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data;

namespace SalonAppointmentSystem
{
    public partial class Form1 : Form
    {
        private readonly List<Appointment> appointments = new List<Appointment>();

        public Form1()
        {
            InitializeComponent();
            try
            {
                DatabaseHelper.InitializeDatabase();
                InitializeServices();
                SetupListView();
                CustomizeUI();
                LoadAppointmentsFromDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeServices()
        {
            string[] services = new string[] {
                "Haircut - $30",
                "Hair Coloring - $80",
                "Manicure - $25",
                "Pedicure - $35",
                "Facial - $50"
            };
            comboBox1.Items.AddRange(services);
            comboBox1.SelectedIndex = 0;
        }

        private void SetupListView()
        {
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;

            // Add columns
            listView1.Columns.Add("Customer Name", 150);
            listView1.Columns.Add("Phone", 100);
            listView1.Columns.Add("Service", 150);
            listView1.Columns.Add("Date & Time", 150);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter customer name.", "Validation Error");
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please enter phone number.", "Validation Error");
                return;
            }

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select a service.", "Validation Error");
                return;
            }

            // Validate appointment date
            if (dateTimePicker1.Value < DateTime.Now)
            {
                MessageBox.Show("Please select a future date and time.", "Validation Error");
                return;
            }

            try
            {
                MessageBox.Show("Starting to save appointment...", "Debug"); // Debug message
                
                var appointment = new Appointment
                {
                    CustomerName = textBox1.Text.Trim(),
                    PhoneNumber = textBox2.Text.Trim(),
                    Service = comboBox1.SelectedItem.ToString(),
                    DateTime = dateTimePicker1.Value
                };

                SaveAppointmentToDatabase(appointment);
                LoadAppointmentsFromDatabase(); // Refresh the list
                ClearInputs();

                MessageBox.Show("Appointment booked successfully!", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving appointment: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearInputs()
        {
            textBox1.Clear();
            textBox2.Clear();
            comboBox1.SelectedIndex = 0;
            dateTimePicker1.Value = DateTime.Now;
        }

        private void CustomizeUI()
        {
            this.BackColor = Color.FromArgb(245, 245, 245);

            // Customize the tab control
            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.BackColor = Color.White;
            }
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(100, 40);
            tabControl1.Font = new Font("Arial", 12F, FontStyle.Bold);

            // Customize all labels
            foreach (Control control in tabPage1.Controls)
            {
                if (control is Label label)
                {
                    label.ForeColor = Color.FromArgb(64, 64, 64);
                }
            }

            // Customize text boxes
            var textBoxStyle = new Action<TextBox>(tb =>
            {
                tb.BackColor = Color.FromArgb(245, 245, 245);
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.Font = new Font("Arial", 10F);
            });

            textBoxStyle(textBox1);
            textBoxStyle(textBox2);

            // Customize combo box
            comboBox1.BackColor = Color.FromArgb(245, 245, 245);
            comboBox1.FlatStyle = FlatStyle.Flat;
            comboBox1.Font = new Font("Arial", 10F);

            // Customize date time picker
            dateTimePicker1.CalendarForeColor = Color.FromArgb(64, 64, 64);
            dateTimePicker1.CalendarMonthBackground = Color.White;
            dateTimePicker1.Font = new Font("Arial", 10F);

            // Customize button
            button1.BackColor = Color.FromArgb(64, 64, 64);
            button1.ForeColor = Color.White;
            button1.Font = new Font("Arial", 10F, FontStyle.Bold);
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;

            // Customize ListView
            listView1.BackColor = Color.White;
            listView1.ForeColor = Color.FromArgb(64, 64, 64);
            listView1.Font = new Font("Arial", 10F);
            listView1.GridLines = true;
            listView1.OwnerDraw = true;
            listView1.DrawColumnHeader += ListViewColumnHeader_Draw;
            listView1.DrawSubItem += ListView1_DrawSubItem;
        }

        private void ListViewColumnHeader_Draw(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(245, 245, 245)), e.Bounds);
            using var font = new Font("Arial", 10F, FontStyle.Bold);
            e.Graphics.DrawString(e.Header.Text, font, new SolidBrush(Color.FromArgb(64, 64, 64)),
                new Rectangle(e.Bounds.X + 5, e.Bounds.Y, e.Bounds.Width - 5, e.Bounds.Height),
                new StringFormat { LineAlignment = StringAlignment.Center });
        }

        private void ListView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(e.ItemState.HasFlag(ListViewItemStates.Selected)
                ? Color.FromArgb(230, 230, 230)
                : Color.White), e.Bounds);

            using var font = new Font("Arial", 10F);
            e.Graphics.DrawString(e.SubItem.Text, font, new SolidBrush(Color.FromArgb(64, 64, 64)),
                new Rectangle(e.Bounds.X + 5, e.Bounds.Y, e.Bounds.Width - 5, e.Bounds.Height),
                new StringFormat { LineAlignment = StringAlignment.Center });
        }

        private bool IsTimeSlotAvailable(DateTime appointmentTime)
        {
            return !appointments.Any(a => 
                a.DateTime.Date == appointmentTime.Date && 
                Math.Abs((a.DateTime - appointmentTime).TotalMinutes) < 30);
        }

        private void SaveAppointmentToDatabase(Appointment appointment)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Database opened successfully", "Debug");
                    
                    string query = @"INSERT INTO appointments 
                        (customer_name, phone_number, service, appointment_datetime) 
                        VALUES (@name, @phone, @service, @datetime)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        // Log parameter values
                        MessageBox.Show($"Saving appointment:\nName: {appointment.CustomerName}\nPhone: {appointment.PhoneNumber}\nService: {appointment.Service}\nDate: {appointment.DateTime}", "Debug Values");
                        
                        cmd.Parameters.AddWithValue("@name", appointment.CustomerName);
                        cmd.Parameters.AddWithValue("@phone", appointment.PhoneNumber);
                        cmd.Parameters.AddWithValue("@service", appointment.Service);
                        cmd.Parameters.AddWithValue("@datetime", appointment.DateTime);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        MessageBox.Show($"Rows affected: {rowsAffected}", "Debug Info");
                        
                        if (rowsAffected > 0)
                        {
                            // Get the last inserted ID
                            cmd.CommandText = "SELECT LAST_INSERT_ID()";
                            int lastId = Convert.ToInt32(cmd.ExecuteScalar());
                            MessageBox.Show($"Last inserted ID: {lastId}", "Debug Info");
                            
                            // Verify the insert
                            cmd.CommandText = "SELECT * FROM appointments WHERE id = @id";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@id", lastId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    MessageBox.Show("Data verified in database!", "Success");
                                }
                                else
                                {
                                    MessageBox.Show("Data not found after insert!", "Warning");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("No rows were inserted!", "Warning");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Error saving to database: {ex.Message}\nError Code: {ex.Number}", 
                        "Database Error", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    throw;
                }
            }
        }

        private void LoadAppointmentsFromDatabase()
        {
            listView1.Items.Clear();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM appointments ORDER BY appointment_datetime";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ListViewItem(reader["customer_name"].ToString());
                        item.SubItems.AddRange(new[] {
                            reader["phone_number"].ToString(),
                            reader["service"].ToString(),
                            Convert.ToDateTime(reader["appointment_datetime"]).ToString("MM/dd/yyyy hh:mm tt")
                        });
                        listView1.Items.Add(item);
                    }
                }
            }
        }
    }

    [Serializable]
    public class Appointment
    {
        public int Id { get; set; }
        public required string CustomerName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Service { get; set; }
        public DateTime DateTime { get; set; }
    }
}