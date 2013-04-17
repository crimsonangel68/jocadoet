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

namespace SpreadsheetGUI
{
    /// <summary>
    /// Partial class for dealing with an open prompt window.
    /// </summary>
    public partial class OpenPrompt : Form
    {
        private String name;
        private bool FAILmessage;
        StringSocket socket;
        private String IPAddress;

        /// <summary>
        /// This method will open a new prompt window, which a user
        ///  will be able to enter in a file to either create a new
        ///  spreadsheet of or join an existing file.
        /// </summary>
        public OpenPrompt()
        {
            // Initialize the window and store the model that was passed in.
            InitializeComponent();
            FAILmessage = false;
            socket = null;
        } // End of "OpenPrompt" method .......................................................................................

        /// <summary>
        /// Used to connect the client to the server
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(String hostname)
        {
            // If the socket has been initialized, create a connection
            if (socket == null)
            {
                // Try to connect to the server.
                TcpClient client = new TcpClient(hostname, 1984);
                socket = new StringSocket(client.Client, UTF8Encoding.UTF8);
                IPAddress = hostname;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenPrompt_FormClosing(object sender, FormClosingEventArgs e)
        {
            socket.CloseSocket();
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
            // Create the message according to protocol
            String message = "CREATE\n";
            message += "Name:" + FileNameTextBox.Text + "\n";
            message += "Password:" + PasswordTextBox.Text + "\n";
            
            // Send the message and then begin receiving
            socket.BeginSend(message, (f, p) => { }, 0);
            socket.BeginReceive(createReceived, 0);
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
            // Crreate the correct message according to protocol
            String message = "JOIN\n";
            message += "Name:" + FileNameTextBox.Text + "\n";
            message += "Password:" + PasswordTextBox.Text + "\n";

            // Send the message to the server and then begin receiving
            socket.BeginSend(message, (f, p) => { }, 0);
            socket.BeginReceive(joinReceived, 0);
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
            // CREATE SP OK/FAIL LF
            // Name:name LF
            // (Password:password LF) / (message LF)

            //string pattern = @"^[:-\n] $";
            //string[] tokens = Regex.Split(s, @"^");
            
            string password;
            
            if (s.Contains("CREATE"))
            {
                if (s.Contains("FAIL"))
                {
                    FAILmessage = true;
                }
                // Continue receiving on the socket
                socket.BeginReceive(createReceived, null);
            }
            else if (s.Contains("Name:"))
            {
                name = s.Substring(5);
                // Continue receiving on the socket
                socket.BeginReceive(createReceived, null);
            }
            // Check to see if the message sent back failed
            else if (FAILmessage)
            {
                // Report to the user the message sent from the server
                DialogResult result = MessageBox.Show(s, "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                // If the user decides to cancel, disconnect the socket and close the prompts
                if (result == DialogResult.Cancel)
                {
                    socket.CloseSocket();
                }
            }
            // If the string contains Password:, we know that we have completed a successful transmission
            else if (s.Contains("Password:"))
            {
                // We show the password returned from the server, along with the name of the spreadsheet returned from the server.
                //  We then ask for confirmation on whether or not they want to join the specified spreadsheet.
                password = s.Substring(9);
                DialogResult result = MessageBox.Show("Spreadsheet name: " + name + "\nSpreadsheet password: " + password, "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                // If the user wants to proceed, send a join request
                if (result == DialogResult.OK)
                {
                    // Crreate the correct message according to protocol
                    String message = "JOIN\n";
                    message += "Name:" + FileNameTextBox.Text + "\n";
                    message += "Password:" + PasswordTextBox.Text + "\n";

                    // Send the message to the server and then begin receiving
                    socket.BeginSend(message, (f, q) => { }, 0);
                    socket.BeginReceive(joinReceived, 0);
                }
            }

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
            // JOIN SP OK/FAIL LF
            // Name:name LF
            // (Version:version LF) / (message LF)
            // (Length:length LF) / 
            // (xml LF) / 

            int length;
            if (s.Contains("JOIN"))
            {
                if (s.Contains("FAIL"))
                {
                    FAILmessage = true;
                }
                // Continue receiving on the socket
                socket.BeginReceive(joinReceived, null);
            }
            else if (s.Contains("Name:"))
            {
                name = s.Substring(5);
                // Continue receiving on the socket
                socket.BeginReceive(joinReceived, null);
            }
            // Check to see if the message sent back failed
            else if (FAILmessage)
            {
                // Report to the user the message sent from the server
                DialogResult result = MessageBox.Show(s, "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                // If the user decides to cancel, disconnect the socket and close the prompts
                if (result == DialogResult.Cancel)
                {
                    socket.CloseSocket();
                }
            }
            else if (s.Contains("Version:"))
            {
                // FIGURE OUT HOW TO SET/GET VERSION
                // Continue receiving on the socket
                socket.BeginReceive(joinReceived, null);
            }
            else if (s.Contains("Length:"))
            {
                Int32.TryParse(s.Substring(7), out length);
                // Continue receiving on the socket
                socket.BeginReceive(joinReceived, null);
            }
            else
            {
                //Form1 spreadsheet = new Form1(xml);

                // OPEN XML FILE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
        } // End of "JoinReceived" method ..........................................................................................
    }
}
