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
        private String name;        // This will store the name of the file inputed to the screen
        private String IPAddress;   // This will store the IP address inputed to the screen
        private String version;

        private bool FAILmessage;   // This flag will be used in the callback methods
        private int length;         // This value will store the length of the xml that will be received
        
        StringSocket socket;        // This socket will be used to communicate to the server

        bool receiving;
        object locker;

        /// <summary>
        /// This method will open a new prompt window, which a user
        ///  will be able to enter in a file to either create a new
        ///  spreadsheet of or join an existing file.
        /// </summary>
        /// <param name="IP"></param>
        public OpenPrompt(String IP)
        {
            // Initialize the window and set up the member variables.
            InitializeComponent();
            FAILmessage = false;
            socket = null;
            receiving = false;

            locker = 0;

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
                // Disconnect the socket gracefully.
                //socket.CloseSocket();
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
            // Create the message according to protocol
            String message = "CREATE \n";
            message += "Name:" + FileNameTextBox.Text + " \n";
            message += "Password:" + PasswordTextBox.Text + " \n";
            
            if (!receiving)
            {
                // Send the message and then begin receiving
                socket.BeginSend(message, (f, p) => { }, 0);

                //receiving = true;
                socket.BeginReceive(createReceived, 0);
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
            // Crreate the correct message according to protocol
            String message = "JOIN \n";
            message += "Name:" + FileNameTextBox.Text + " \n";
            message += "Password:" + PasswordTextBox.Text + " \n";

            if (!receiving)
            {
                // Send the message to the server and then begin receiving
                socket.BeginSend(message, (f, p) => { }, 0);

                //receiving = true;
                socket.BeginReceive(joinReceived, 0);
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
            if (s.Contains("CREATE SP FAIL"))
                socket.BeginReceive(failMethod, 0);

            else if (s.StartsWith("CREATE SP OK"))
                socket.BeginReceive(createReceived, 0);
            
            else if (s.StartsWith("Name:"))
            {
                name = s.Substring(5);
                
                // Continue receiving on the socket
                socket.BeginReceive(createReceived, 0);
            }
            
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
                    socket.BeginReceive(joinReceived, 0);
                }
                else
                    receiving = false;

                
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
            //lock (locker)
            {
                if (s.Contains("xml"))
                {
                    MessageBox.Show(s);

                    SpreadsheetApplicationContext appContext = SpreadsheetApplicationContext.getAppContext();
                    //appContext.RunForm(new Form1(IPAddress, name, version, s, socket));
                    this.BeginInvoke(new Action(() => { (new Form1(IPAddress, name, version, s, socket)).ShowDialog(); }));
                    this.BeginInvoke(new Action(() => { Close(); }));
                    //Application.Run(appContext);
                    //Application.Run(new Form1(IPAddress, name, version, s, socket));
                    
                    //ThreadPool.QueueUserWorkItem(x => (new Form1(IPAddress, name, version, s, socket)).Show());
                }
                else if (s.Contains("JOIN"))
                {
                    if (s.Contains("FAIL"))
                        FAILmessage = true;

                    // Continue receiving on the socket
                    socket.BeginReceive(joinReceived, 0);
                }
                else if (s.Contains("Name:"))
                {
                    name = s.Substring(5);
                    // Continue receiving on the socket
                    socket.BeginReceive(joinReceived, 0);
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
                    version = s.Substring(8);

                    // Continue receiving on the socket
                    socket.BeginReceive(joinReceived, 0);
                }
                else if (s.Contains("Length:"))
                {
                    Int32.TryParse(s.Substring(7), out length);

                    // Continue receiving on the socket
                    socket.BeginReceive(joinReceived, 0);
                }
            }
        } // End of "JoinReceived" method ..........................................................................................

        /// <summary>
        /// 
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
                    socket.CloseSocket();
            }
        }
    }
}
