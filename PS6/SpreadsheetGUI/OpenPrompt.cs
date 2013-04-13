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

        private void NewButton_Click(object sender, EventArgs e)
        {
            String message = "CREATE\n";
            message += "Name:" + FileNameTextBox.Text + "\n";
            message += "Password:" + PasswordTextBox.Text + "\n";
            model.topModel.socket.BeginSend(message, (f, p) => { }, 0);
            model.topModel.socket.BeginReceive(lineReceived, 0);
        }

        private void JoinButton_Click(object sender, EventArgs e)
        {
            model.topModel.socket.BeginSend(FileNameTextBox.Text, (f, p) => { }, 0);
            model.topModel.socket.BeginReceive(lineReceived, 0);
        }

        private void lineReceived(String s, Exception e, object p)
        {

        }
    }
}
