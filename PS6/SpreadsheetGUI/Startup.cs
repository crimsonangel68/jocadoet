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
    /// This partial class will .....
    /// </summary>
    public partial class Startup : Form
    {
        private StringSocket socket;

        /// <summary>
        ///  This is the constructor for the startup method.
        ///  
        /// It will create a new socket to connect to the provided
        ///  IP address on the specified port #.  It will then pass
        ///  along the socket to the OPEN prompt, where the user will
        ///  specify the file name and whether they will create a new
        ///  file, or join an existing file.
        /// </summary>
        public Startup()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (IPTextBox.Text != "" && PortTextBox.Text != "")
                {
                    // connect to server with open prompt as callback.
                }
            }
            catch (Exception)
            {
                DialogResult result = MessageBox.Show("Invalid IPAddress/Port or Server is currently not running.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
        }
    }
}
