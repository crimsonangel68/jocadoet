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
        /// <summary>
        /// This method will open a new prompt window, which a user
        ///  will be able to enter in a file to either create a new
        ///  spreadsheet of or join an existing file.
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="PortNum"></param>
        public OpenPrompt(String IPAddress, String PortNum)
        {
            InitializeComponent();
        }
    }
}
