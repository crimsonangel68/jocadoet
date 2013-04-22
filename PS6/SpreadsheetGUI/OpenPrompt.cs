using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using CustomNetworking;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Partial class for dealing with an open prompt window.
    /// </summary>
    public partial class OpenPrompt : Form
    {
        StringSocket socket;        // This socket will be used to communicate to the server
        SpreadsheetApplicationContext app;

        private String name;        // This will store the name of the file inputed to the screen
        private String IPAddress;   // This will store the IP address inputed to the screen
        private String version;     // This will store the version sent in from the server
        private int length;         // This will store the length of the xml that will be received
        
        bool receiving;             // This flag will keep the client from sending messages while it's receiving

        /// <summary>
        /// This method will open a new prompt window, which a user
        ///  will be able to enter in a file to either create a new
        ///  spreadsheet of or join an existing file.
        /// </summary>
        /// <param name="IP"></param>
        public OpenPrompt(String IP, SpreadsheetApplicationContext appContext)
        {
            // Initialize the window and set up the member variables.
            InitializeComponent();
            socket = null;
            receiving = false;
            app = appContext;

            // Connect to the provided IP address
            Connect(IP);
        } // End of "OpenPrompt" method .......................................................................................

        /// <summary>
        /// This method is used to try and connect the client to the server, provided
        ///  an IP address to connect to.
        ///  
        /// The caller method will need to catch any exceptions thrown by an error
        ///  connecting.
        /// </summary>
        /// <param name="hostname"></param>
        public void Connect(String hostname)
        {
            // If the socket has been initialized, create a connection
            if (socket == null)
            {
                // Try to connect to the server.
                TcpClient client = new TcpClient(hostname, 1984);
                socket = new StringSocket(client.Client, UTF8Encoding.UTF8);

                // Store the provided host address into the IP address member variable
                IPAddress = hostname;
            }
        }

        /// <summary>
        /// This method is called when the user tries to close the window, after a connection 
        ///  has been made.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenPrompt_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                socket.CloseSocket();  // Disconnect the socket gracefully.
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// This method is for when the "New" button is clicked in the GUI.
        /// 
        /// It will send the appropiate message to the client, and begin receiving
        ///  with the correct callback method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewButton_Click(object sender, EventArgs e)
        {
            if (!FileNameTextBox.Text.Equals("") && !PasswordTextBox.Text.Equals(""))
            {
                // Create the message according to protocol
                String message = "CREATE \n";
                message += "Name:" + FileNameTextBox.Text + " \n";
                message += "Password:" + PasswordTextBox.Text + " \n";

                if (!receiving)
                {
                    receiving = true;

                    try
                    {
                        // Send the message and then begin receiving
                        socket.BeginSend(message, (f, p) => { }, 0);
                        socket.BeginReceive(createReceived, "CREATE");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Server is not responding, please wait or close and reconnect");
                    }
                }
            }
        } // End of "NewButton_Click" method .............................................................................

        /// <summary>
        /// This method is for when the "Join" button is clicked in the GUI.
        /// 
        /// It will send the appropiate message to the server and begin receiving
        ///  with the correct callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JoinButton_Click(object sender, EventArgs e)
        {
            if (!FileNameTextBox.Text.Equals("") && !PasswordTextBox.Text.Equals(""))
            {
                // Crreate the correct message according to protocol
                String message = "JOIN \n";
                message += "Name:" + FileNameTextBox.Text + " \n";
                message += "Password:" + PasswordTextBox.Text + " \n";

                if (!receiving)
                {
                    receiving = true;

                    try
                    {
                        // Send the message to the server and then begin receiving
                        socket.BeginSend(message, (f, p) => { }, 0);
                        socket.BeginReceive(joinReceived, "JOIN");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Server is not responding, please wait or close and reconnect");
                    }
                }
            }
        } // End of "JoinButton_Click" method .....................................................................................................

        /// <summary>
        /// This is the callback method for the "Create" message.
        /// 
        /// This will report to the user whether they will be able to
        ///  create a new spreadsheet, or if there's an error that
        ///  occurred.
        ///  
        /// If there was an error it reports the error to the user, if
        ///  the connection was successful it will report to the user the 
        ///  name and spreadsheet.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void createReceived(String s, Exception e, object p)
        {
            if (s.StartsWith("CREATE FAIL") || s.StartsWith("\0CREATE FAIL"))
            {
                socket.BeginReceive(failMethod, "FAIL");
                return;
            }
            else if (s.StartsWith("Name:"))
                name = s.Substring(5);

            // If the string contains Password:, we know that we have completed a successful transmission
            else if (s.StartsWith("Password:"))
            {
                // We show the password returned from the server, along with the name of the spreadsheet returned from the server.
                //  We then ask for confirmation on whether or not they want to join the specified spreadsheet.
                DialogResult result = MessageBox.Show("Spreadsheet name: " + name + "\nSpreadsheet password: " + s.Substring(9), "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                // If the user wants to proceed, send a join request
                if (result == DialogResult.OK)
                {
                    // Crreate the correct message according to protocol
                    String message = "JOIN \n";
                    message += "Name:" + name + " \n";
                    message += "Password:" + PasswordTextBox.Text + " \n";

                    // Send the message to the server and then begin receiving
                    socket.BeginSend(message, (f, q) => { }, 0);
                    socket.BeginReceive(joinReceived, "CREATE");
                }
                else
                    receiving = false;

                return;
            }
            
            socket.BeginReceive(createReceived, "CREATE");
        } // End of "createReceived" method ............................................................................................................

        /// <summary>
        /// This method is the callback for the "Join" message.
        /// 
        /// This will report to the user whether they will be able to
        ///  join a spreadsheet, or if there's an error that
        ///  occurred.
        ///  
        /// If there was an error it reports the error to the user, if
        ///  the connection was successful it will report to the user the 
        ///  name and spreadsheet.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void joinReceived(String s, Exception e, object p)
        {
            if (s.Contains("xml"))
            {
                // We've received the xml, create a new form1 and show it
                Form1 ss = new Form1(IPAddress, name, version, s, socket, app);

                this.BeginInvoke(new Action(() => { this.Hide(); }));
                this.Invoke(new Action(() => { ss.ShowDialog(); }));

                return;
            }

            else if (s.StartsWith("JOIN FAIL") || s.StartsWith("\0JOIN FAIL"))
            {
                socket.BeginReceive(failMethod, "FAIL");
                return;
            }
            else if (s.StartsWith("Name:"))
                name = s.Substring(5);

            else if (s.StartsWith("Version:"))
                version = s.Substring(8);

            else if (s.StartsWith("Length:"))
                Int32.TryParse(s.Substring(7), out length);

            socket.BeginReceive(joinReceived, "JOIN");
        } // End of "JoinReceived" method ..........................................................................................

        /// <summary>
        /// This method is for dealing with receiving fails.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void failMethod(String s, Exception e, Object p)
        {
            if (!s.StartsWith("Name:"))
            {
                receiving = false;

                // Report to the user the message sent from the server
                DialogResult result = MessageBox.Show(s, "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                // If the user decides to cancel, disconnect the socket and close the prompts
                if (result == DialogResult.Cancel)
                    this.Invoke(new Action(() => { Close(); }));

                return;
            }

            socket.BeginReceive(failMethod, "FAIL");
        }
    }
}
