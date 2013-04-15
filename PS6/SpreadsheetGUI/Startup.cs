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
        StartupModel model;

        /// <summary>
        /// This property will keep track of the IP Adress that the
        ///  user puts in.
        /// </summary>
        public string IPAddress { get; set; }
        
        /// <summary>
        /// This property will keep track of the Port number that was entered
        ///  by the user.
        /// </summary>
        public int PortNum   { get; set; }

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
            // Initialize this prompt and create a new model
            InitializeComponent();
            model = new StartupModel();
        }

        /// <summary>
        /// This method will connect the client to the server. The
        ///  user will then be directed to the next prompt, the open
        ///  prompt.
        /// </summary>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            // Try to connect to the server, if unsuccessful, then show
            //  dialog box reporting the error to the user
            try
            {
                // If both of the fields in the window contain elements
                if (IPTextBox.Text != "" && PortTextBox.Text != "")
                {
                    // Store the IP address
                    IPAddress = IPTextBox.Text;
                    
                    // create a variable to store the value of the port number
                    int pNum = 0;

                    // Try to parse out the number of the portTextBox, if unseccesful, throw an error.
                    if (!Int32.TryParse(PortTextBox.Text, out pNum))
                        throw new Exception();

                    // Put the port number into the property
                    PortNum = pNum;

                    // Connect to the server and open the next prompt
                    SSModel connectedModel = model.Connect(IPAddress, PortNum);
                    GoToOpenPrompt(connectedModel);
                }
            }
            // Catch any exception to report to the user
            catch (Exception)
            {
                // Display a message informing the user that the inputs were invalid or the server is off
                DialogResult result = MessageBox.Show("Invalid IPAddress/Port or Server is currently not running.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                    this.Close();
            }
        }
        
        /// <summary>
        /// This method is the callback for connecting to the server.
        ///  It will then direct the user to the next prompt to 
        ///  open/create a file.
        /// </summary>
        public void GoToOpenPrompt(SSModel spreadsheetModel)
        {
            // Create the next prompt window
            OpenPrompt prompt = new OpenPrompt(spreadsheetModel);

            // Show the next prompt and close this window
            this.Hide();
            prompt.ShowDialog();
            this.Close();
        }

    }
}
