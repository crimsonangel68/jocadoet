using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CustomNetworking;

namespace SpreadsheetGUI
{
    /// <summary>
    /// This partial class will run when the program is executed.
    /// 
    /// It will show a window that allows the user to input an IP address
    ///  to attempt to connect to.
    ///  
    /// If the connection was unsuccessful, it will inform the user.
    /// </summary>
    public partial class Startup : Form
    {
        SpreadsheetApplicationContext app;
        /// <summary>
        ///  This is the constructor for the startup method.
        ///  
        /// It will create a new socket to connect to the provided
        ///  IP address on the specified port #.  It will then pass
        ///  along the socket to the OPEN prompt, where the user will
        ///  specify the file name and whether they will create a new
        ///  file, or join an existing file.
        /// </summary>
        public Startup(SpreadsheetApplicationContext appContext)
        {
            // Initialize this prompt and create a new model
            InitializeComponent();
            app = appContext;
        }

        /// <summary>
        /// This method will connect the client to the server. The
        ///  user will then be directed to the next prompt, the open
        ///  prompt.
        /// </summary>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            // If both of the fields in the window contain elements
            if (IPTextBox.Text != "")
            {
                // Store the IP address
                String IPAddress = IPTextBox.Text;

                GoToOpenPrompt(IPAddress);
            }
        }
        
        /// <summary>
        /// This method is the callback for connecting to the server.
        ///  It will then direct the user to the next prompt to 
        ///  open/create a file.
        /// </summary>
        public void GoToOpenPrompt(String IPAddress)
        {
            try
            {
                // Create the next prompt window
                OpenPrompt prompt = new OpenPrompt(IPAddress, app);

                // Show the next prompt and close this window
                this.Hide();
                app.RunForm(prompt);
                //prompt.ShowDialog();
                this.Close();
            }
            catch (Exception)
            {
                // Display a message informing the user that the inputs were invalid or the server is off
                DialogResult result = MessageBox.Show("Invalid IPAddress/Port or Server is currently not running.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel || !this.Visible)
                    this.Close();
            }
        }

    }
}
