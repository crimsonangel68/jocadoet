using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Partial class for dealing with an open prompt window.
    /// </summary>
    public partial class OpenPrompt : Form
    {
        SSModel model;
        bool FAILmessage;
        
        /// <summary>
        /// This method will open a new prompt window, which a user
        ///  will be able to enter in a file to either create a new
        ///  spreadsheet of or join an existing file.
        /// </summary>
        /// <param name="newModel" ></param>
        public OpenPrompt(SSModel newModel)
        {
            // Initialize the window and store the model that was passed in.
            InitializeComponent();
            model = newModel;
            FAILmessage = false;
        } // End of "OpenPrompt" method .......................................................................................

        private void OpenPrompt_FormClosing(object sender, FormClosingEventArgs e)
        {
            model.startupModel.socket.CloseSocket();
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
            model.startupModel.socket.BeginSend(message, (f, p) => { }, 0);
            model.startupModel.socket.BeginReceive(createReceived, 0);
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
            model.startupModel.socket.BeginSend(message, (f, p) => { }, 0);
            model.startupModel.socket.BeginReceive(joinReceived, 0);
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
            //MessageBox.Show(s);

            if (s.Contains("CREATE"))
            {
                if (s.Contains("FAIL"))
                {
                    FAILmessage = true;
                }
                // Continue receiving on the socket
                model.startupModel.socket.BeginReceive(createReceived, null);
            }
            else if (s.Contains("Name:"))
            {
                model.name = s.Substring(5);
                // Continue receiving on the socket
                model.startupModel.socket.BeginReceive(createReceived, null);
            }
            // Check to see if the message sent back failed
            else if (FAILmessage)
            {
                // Report to the user the message sent from the server
                DialogResult result = MessageBox.Show(s, "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                // If the user decides to cancel, disconnect the socket and close the prompts
                if (result == DialogResult.Cancel)
                {
                    model.startupModel.socket.CloseSocket();
                }
            }
            else if (s.Contains("Password:"))
            {
                password = s.Substring(9);
                MessageBox.Show("Spreadsheet name: " + model.name + "\nSpreadsheet password: " + password);
                // OPEN FILE
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
                model.startupModel.socket.BeginReceive(joinReceived, null);
            }
            else if (s.Contains("Name:"))
            {
                model.name = s.Substring(5);
                // Continue receiving on the socket
                model.startupModel.socket.BeginReceive(joinReceived, null);
            }
            // Check to see if the message sent back failed
            else if (FAILmessage)
            {
                // Report to the user the message sent from the server
                DialogResult result = MessageBox.Show(s, "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                // If the user decides to cancel, disconnect the socket and close the prompts
                if (result == DialogResult.Cancel)
                {
                    model.startupModel.socket.CloseSocket();
                }
            }
            else if (s.Contains("Version:"))
            {
                // FIGURE OUT HOW TO SET/GET VERSION
                // Continue receiving on the socket
                model.startupModel.socket.BeginReceive(joinReceived, null);
            }
            else if (s.Contains("Length:"))
            {
                Int32.TryParse(s.Substring(7), out length);
                // Continue receiving on the socket
                model.startupModel.socket.BeginReceive(joinReceived, null);
            }
            else
            {
                // OPEN XML FILE
            }
        } // End of "JoinReceived" method ..........................................................................................
    }
}
