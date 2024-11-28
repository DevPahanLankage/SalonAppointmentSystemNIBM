using System;
using System.Windows.Forms;

namespace SalonAppointmentSystem
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", 
                    "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);                 
            }
        }
    }
}