/*
 *
 * This project was originally an exercise in CS3500 Fall 2012 at the University of Utah completed
 * by Joshua Boren.
 * 
 * The project is now being extended to be used with a server that is being built in our CS3505
 * Spring 2013 course.  Changes will be made by Joshua Boren, Calvin Kern, Doug Hitchcock and 
 * Ethan Hayes to allow this front end of the spreadsheet to have collaboration functionality
 * for users across networks.
 * 
 * Calvin was here!
 *
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SS;
using CustomNetworking;

namespace SpreadsheetGUI
{
    /// <summary>
    /// A GUI representation of a Spreadsheet.  Allows the user to navigate and manipulate the 
    /// contents of a spreadsheet.
    /// </summary>
    public partial class Form1 : Form
    {
        private Spreadsheet sheet;
        private List<string> alphabet = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private SSModel model;
        private StringSocket socket;

        //-----------------------------------------------------------------------------------Form1
        /// <summary>
        /// Opens a new Window with an empty spreadsheet.
        /// </summary>
        public Form1(StringSocket thisSocket, String fileName, String currentVersion)
        {
            InitializeComponent();
            socket = thisSocket;
            model = new SSModel(socket);

            sheet.FileName = fileName;
            sheet = new Spreadsheet(s => true, s => s.ToUpper(), currentVersion);

            spreadsheetPanel1.SelectionChanged += updateSelection;
            spreadsheetPanel1.SetSelection(0, 0);
            updateSelection(spreadsheetPanel1);

        }

        //-----------------------------------------------------------------------------------Form1(pathname)
        /// <summary>
        /// Opens a new Window when a file with a .ss extension is opened.  The Window will contain
        /// the contents of the file opened.
        /// </summary>
        public Form1(string pathname)
        {
            InitializeComponent();

            sheet = new Spreadsheet(pathname, s => true, s => s.ToUpper(), "ps6");

            IEnumerable<string> cellNames = sheet.GetNamesOfAllNonemptyCells();
            foreach (string name in cellNames)
            {
                int col, row;
                col = alphabet.IndexOf(name.Substring(0, 1));
                row = int.Parse(name.Substring(1)) - 1;

                spreadsheetPanel1.SetValue(col, row, sheet.GetCellValue(name).ToString());
                updateCell(spreadsheetPanel1, col, row);
            }
        }

        //-----------------------------------------------------------------------------------updateSelection
        /// <summary>
        /// Updates the selection.
        /// </summary>
        /// <param name="panel"></param>
        private void updateSelection(SpreadsheetPanel panel)
        {
            int row, col;
            panel.GetSelection(out col, out row);
            string name = alphabet[col] + (row + 1).ToString();

            CellNameBox.Text = name;

            CellValueBox.Text = (sheet.GetCellValue(name)).ToString();

            if (CellValueBox.Text == "")
            {
                CellContentBox.Clear();
            }
            else
            {
                if (sheet.GetCellContents(name) is SpreadsheetUtilities.Formula)
                    CellContentBox.Text = "=" + (sheet.GetCellContents(name)).ToString();
                else
                    CellContentBox.Text = (sheet.GetCellContents(name)).ToString();

                panel.SetValue(col, row, (sheet.GetCellValue(name)).ToString());
            }
        }

        //-----------------------------------------------------------------------------------newToolStripMenuItem_Click
        /// <summary>
        /// Creates a new Window and new spreadsheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpreadsheetApplicationContext.getAppContext().RunForm(new Form1());
        }

        //-----------------------------------------------------------------------------------openToolStripMenuItem_Click
        /// <summary>
        /// Open a saved spreadsheet file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Spreadsheet Files (.ss)|*.ss|All Files (*.*)|*.*";
            openFileDialog.Title = "Open Spreadsheet";
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.RestoreDirectory = false;

            DialogResult userClickedOK = openFileDialog.ShowDialog();
            if (userClickedOK == DialogResult.OK)
            {
                string filepath = openFileDialog.FileName;

                Form1 openForm = new Form1();

                SpreadsheetApplicationContext.getAppContext().RunForm(openForm);
                openForm.sheet = new Spreadsheet(filepath, s => true, s => s.ToUpper(), "ps6");

                IEnumerable<string> cellNames = openForm.sheet.GetNamesOfAllNonemptyCells();
                foreach (string name in cellNames)
                {
                    int col, row;
                    col = alphabet.IndexOf(name.Substring(0, 1));
                    row = int.Parse(name.Substring(1)) - 1;

                    openForm.spreadsheetPanel1.SetValue(col, row, openForm.sheet.GetCellValue(name).ToString());
                    openForm.updateCell(openForm.spreadsheetPanel1, col, row);
                }
            }
        }

        //-----------------------------------------------------------------------------------saveToolStripMenuItem_Click
        /// <summary>
        /// Save a version of the spreadsheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Spreadsheet Files (.ss)|*.ss|All Files (*.*)|*.*";
            saveFileDialog1.Title = "Save";
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.OverwritePrompt = true;

            DialogResult userClickedOK = saveFileDialog1.ShowDialog();
            if (userClickedOK == DialogResult.OK)
            {
                string filepath = saveFileDialog1.FileName;
                sheet.Save(filepath);
            }
        }

        //-----------------------------------------------------------------------------------closeToolStripMenuItem_Click
        /// <summary>
        /// Closes the current Form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sheet.Changed && (MessageBox.Show("Do you want to exit without saving?", "SpreadsheetProgram", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
            {
                Close();
            }
            if (!sheet.Changed)
                Close();
        }

        //-----------------------------------------------------------------------------------Help_Click
        /// <summary>
        /// Displays a message box that explains to a user how to use the GUI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Help_Click(object sender, EventArgs e)
        {
            string helpMessage = "This Application mimics an Excel spreadsheet in various ways.";
            helpMessage += "When the Cell Content Box is selected you may manipulate the spreadsheet in the following manner:\n\n";
            helpMessage += "To enter data, you must press the return key before selecting another cell.\n\n";
            helpMessage += "Use the arrow keys to change the current selection of the cell.\n\n";
            helpMessage += "The \"Clear\" button at the top of the window will clear the entire Spreadsheet.\n\n";
            helpMessage += "You may use the shortcut ctrl + w to close the window.\n\n";
            MessageBox.Show(helpMessage);
        }

        //-----------------------------------------------------------------------------------CellContentBox
        /// <summary>
        /// Allows the user to use the keyboard to control data entry from the keyboard.  While the CellContentBox
        /// is selected, the user must use the enter key to input the data into the spreadsheet, and may use the arrows
        /// to navigate the spreadsheet cells.  Also, when control + w is pressed, the option to close the Window will
        /// be presented to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellContentBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                int col, row;
                int newCol, newRow;
                if (e.KeyCode == Keys.Enter)
                {
                    sendMessage(spreadsheetPanel1);
                    SetCellContents(spreadsheetPanel1);
                }

                if (e.KeyCode == Keys.Left)
                {
                    spreadsheetPanel1.GetSelection(out col, out row);
                    if (col != 0)
                        newCol = col - 1;
                    else
                        newCol = col;
                    newRow = row;

                    NavigateUpdate(newCol, newRow);
                }
                if (e.KeyCode == Keys.Right)
                {
                    spreadsheetPanel1.GetSelection(out col, out row);
                    if (col != 25)
                        newCol = col + 1;
                    else
                        newCol = col;
                    newRow = row;

                    NavigateUpdate(newCol, newRow);
                }
                if (e.KeyCode == Keys.Up)
                {
                    spreadsheetPanel1.GetSelection(out col, out row);
                    newCol = col;
                    newRow = row - 1;

                    NavigateUpdate(newCol, newRow);
                }
                if (e.KeyCode == Keys.Down)
                {
                    spreadsheetPanel1.GetSelection(out col, out row);
                    newCol = col;
                    newRow = row + 1;

                    NavigateUpdate(newCol, newRow);
                }
                if (e.Control && e.KeyCode == Keys.W)
                {
                    Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Idiot... ");
            }
        }

        //-----------------------------------------------------------------------------------SetCellContents
        /// <summary>
        /// A helper method to set the contents of the cell in the spreadsheetPanel.  This will
        /// actually display on the panel the inputed information.
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        private void SetCellContents(SpreadsheetPanel panel)
        {
            try
            {
                int col, row;
                panel.GetSelection(out col, out row);
                string name = alphabet[col] + (row + 1).ToString();

                string contents = CellContentBox.Text;
                ISet<string> affectedCells = sheet.SetContentsOfCell(name, contents);

                // Send change here!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Change LF
                // Name:name LF
                // Version:version LF
                // Cell:cell LF
                // Length:length LF
                // content LF

                // socket.beginSend("^^\n");

                foreach (string cell in affectedCells)
                {
                    int c, r;
                    c = alphabet.IndexOf(cell.Substring(0, 1));
                    r = int.Parse(cell.Substring(1)) - 1;

                    // Change this to sending to server !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    panel.SetValue(c, r, sheet.GetCellValue(cell).ToString());

                }
                string value = sheet.GetCellValue(name).ToString();

                panel.SetValue(col, row, value);

                // Make this a callback for sending the server and have it set the value for the panel as well!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                updateSelection(panel);

            }
            catch (Exception f)
            {
                MessageBox.Show("Idiot.... (SetCellContents) \n" + f.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="panel"></param>
        private void sendMessage(SpreadsheetPanel panel)
        {
            //try
            //{
            //    int col, row;
            //    panel.GetSelection(out col, out row);
            //    string name = alphabet[col] + (row + 1).ToString();

            //    object oldContent = sheet.GetCellContents(name);
                
            //    string contents = CellContentBox.Text;
            //    ISet<string> affectedCells = sheet.SetContentsOfCell(name, contents);

            //    foreach (string cell in affectedCells)
            //    {

            //    }
            //}
            //catch (Exception f)
            //{
            //    MessageBox.Show("Idiot... (sendMessage) \n" + f.Message);
            //}
        }

        //-----------------------------------------------------------------------------------Clear_Click
        /// <summary>
        /// When the clear button is clicked on the Window, the spreadsheetPanel, spreadsheet, 
        /// CellContentBox and CellValueBox will be cleared.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Click(object sender, EventArgs e)
        {
            CellContentBox.Text = "";
            CellValueBox.Text = "";
            spreadsheetPanel1.Clear();
            IEnumerable<string> list = sheet.GetNamesOfAllNonemptyCells();
            HashSet<string> billy = new HashSet<string>();
            foreach (string s in list)
                billy.Add(s);
            foreach (string name in billy)
                sheet.SetContentsOfCell(name, "");
        }

        //-----------------------------------------------------------------------------------Form1_FormClosing
        /// <summary>
        /// Closes the spreadsheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sheet.Changed && (MessageBox.Show("Do you want to exit without saving?", "SpreadsheetProgram", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
                e.Cancel = false;
            else
                e.Cancel = true;
            if (!sheet.Changed)
                e.Cancel = false;
        }

        //-----------------------------------------------------------------------------------updateCell
        /// <summary>
        /// A helper method to update the contents of the cell in the spreadsheetPanel.
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="col"></param>
        /// <param name="row"></param>
        private void updateCell(SpreadsheetPanel panel, int col, int row)
        {
            string name = alphabet[col] + (row + 1).ToString();
            panel.SetValue(col, row, (sheet.GetCellValue(name)).ToString());

        }

        //-----------------------------------------------------------------------------------updatePanel()
        /// <summary>
        /// A helper method to update the entire spreadsheetPanel when a file is loaded.
        /// </summary>
        private void updatePanel()
        {
            IEnumerable<string> cellNames = sheet.GetNamesOfAllNonemptyCells();
            foreach (string name in cellNames)
            {
                int col, row;
                col = alphabet.IndexOf(name.Substring(0, 1));
                row = int.Parse(name.Substring(1)) - 1;

                spreadsheetPanel1.SetValue(col, row, sheet.GetCellValue(name).ToString());
                updateCell(spreadsheetPanel1, col, row);
            }
        }

        //-----------------------------------------------------------------------------------NavigateUpdate
        /// <summary>
        /// A helper method to update the selection on the spreadsheetPanel when the arrow
        /// keys are used to navigate the spreadsheet.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        private void NavigateUpdate(int col, int row)
        {
            spreadsheetPanel1.SetSelection(col, row);
            updateCell(spreadsheetPanel1, col, row);
            string value;
            spreadsheetPanel1.GetValue(col, row, out value);
            if (value != "")
                CellContentBox.Text = value;
            else
                CellContentBox.Clear();
            updateSelection(spreadsheetPanel1);
        }

        //-----------------------------------------------------------------------------------Form1_Load
        /// <summary>
        /// Empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        //-----------------------------------------------------------------------------------spreadsheetspreadsheetpanel11_Load
        /// <summary>
        /// Empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetspreadsheetpanel1_Load(object sender, EventArgs e)
        {
        }

        //-----------------------------------------------------------------------------------openFileDialog_FileOk
        /// <summary>
        /// Empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }


        //-----------------------------------------------------------------------------------openFileDialog_FileOk
        /// <summary>
        /// Empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
        }
    }
}
