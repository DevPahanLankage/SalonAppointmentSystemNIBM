using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;

namespace SalonAppointmentSystem
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Port=3306;Database=salon_db;Uid=root;Pwd=admin123;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Database connected successfully!", "Success");
                    
                    // Create appointments table if it doesn't exist
                    string createTable = @"
                        CREATE TABLE IF NOT EXISTS appointments (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            customer_name VARCHAR(100) NOT NULL,
                            phone_number VARCHAR(20) NOT NULL,
                            service VARCHAR(100) NOT NULL,
                            appointment_datetime DATETIME NOT NULL
                        )";

                    using (var cmd = new MySqlCommand(createTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Table created/verified successfully!", "Success");
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}\nError Code: {ex.Number}", 
                        "Database Connection Error", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    throw;
                }
            }
        }
    }
} 