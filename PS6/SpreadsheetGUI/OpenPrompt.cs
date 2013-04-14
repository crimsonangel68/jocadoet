using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Partial class for ...
    /// </summary>
    public partial class OpenPrompt : Form
    {
        SSModel model;
        
        /// <summary>
        /// This method will open a new prompt window, which a user
        ///  will be able to enter in a file to either create a new
        ///  spreadsheet of or join an existing file.
        /// </summary>
        /// <param name="newModel" ></param>
        public OpenPrompt(SSModel newModel)
        {
            InitializeComponent();
            model = newModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewButton_Click(object sender, EventArgs e)
        {
            String message = "CREATE\n";
            message += "Name:" + FileNameTextBox.Text + "\n";
            message += "Password:" + PasswordTextBox.Text + "\n";
            model.topModel.socket.BeginSend(message, (f, p) => { }, 0);
            model.topModel.socket.BeginReceive(createReceived, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JoinButton_Click(object sender, EventArgs e)
        {
            String message = "JOIN\n";
            message += "Name:" + FileNameTextBox.Text + "\n";
            message += "Password:" + PasswordTextBox.Text + "\n";
            model.topModel.socket.BeginSend(message, (f, p) => { }, 0);
            model.topModel.socket.BeginReceive(joinReceived, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void createReceived(String s, Exception e, object p)
        {
            // CREATE SP OK/FAIL LF
            // Name:name LF
            // (Passowrd:password LF) / (message LF)
            if (s.Contains("FAIL"))
            {
                DialogResult result = MessageBox.Show(s, "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                {
                    model.topModel.socket.CloseSocket();
                    this.Close();
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(s, "Confirm");
                if (result == DialogResult.Cancel)
                {
                    model.topModel.socket.CloseSocket();
                    this.Close();
                }
                else if (result == DialogResult.Yes)
                {
                    // go open the file
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void joinReceived(String s, Exception e, object p)
        {
            // JOIN SP OK/FAIL LF
            // Name:name LF
            // (Version:version LF) / (message LF)
            // (Length:length LF)
            // (xml LF)
            if (s.Contains("FAIL"))
            {
                DialogResult result = MessageBox.Show(s, "ERROR", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (result == DialogResult.Cancel)
                {
                    model.topModel.socket.CloseSocket();
                    this.Close();
                }
            }

            else
            {
                DialogResult result = MessageBox.Show(s, "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    model.topModel.socket.CloseSocket();
                    this.Close();
                }
                else if (result == DialogResult.Yes)
                {
                    // go open the file
                }
            }
        } // End of "JoinReceived" method /////////////////////////////////////////////
    }
}
