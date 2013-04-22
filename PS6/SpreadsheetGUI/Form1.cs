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
using SpreadsheetUtilities;
using CustomNetworking;
using System.IO;
using System.Threading;

namespace SpreadsheetGUI
{
    /// <summary>
    /// A GUI representation of a Spreadsheet.  Allows the user to navigate
		/// and manipulate the contents of a spreadsheet.
    /// </summary>
    public partial class Form1 : Form
    {
        private Spreadsheet sheet;
        private List<string> alphabet = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        private String modifyingCell;
        private String lengthCell;
        private int messagesToReceive;
        private String SSversion;

        private String IPAddress;
        private StringSocket socket;

        //-------------------------------------------------------Form1(pathname)
        /// <summary>
        /// Opens a new Window when a file with a .ss extension is opened.  
				/// The Window will contain the contents of the file opened.
        /// </summary>
        public Form1(String IP, String fileName, String version, String xml, StringSocket newSocket)
        {
            InitializeComponent();

            this.IPAddress = IP;
            socket = newSocket;
            messagesToReceive = 0;

            String filePath = @"../../../../tmpfiles/jocadoetSpreadsheet.ss";

            //try
            {
                FileInfo newFile = new FileInfo(filePath);

                using (StreamWriter sw = newFile.CreateText())
                {
                    sw.WriteLine(xml);
                }

                sheet = new Spreadsheet(filePath, s => true, s => s.ToUpper(), "ps6");

                sheet.FileName = fileName;
                SSversion = version;

                IEnumerable<string> cellNames = sheet.GetNamesOfAllNonemptyCells();
                foreach (string name in cellNames)
                {
                    int col, row;
                    col = alphabet.IndexOf(name.Substring(0, 1));
                    row = int.Parse(name.Substring(1)) - 1;

                    spreadsheetPanel1.SetValue(col, row, sheet.GetCellValue(name).ToString());
                    updateCell(spreadsheetPanel1, col, row);
                }
                spreadsheetPanel1.SetSelection(0, 0);
                updateSelection(spreadsheetPanel1);
            }
            socket.BeginReceive(MessageReceived, "MESSAGE");
            //catch (Exception e)
            //{
            //    MessageBox.Show("ERROR:\n" + e, "ERROR");
            //}
        }

        //------------------------------------------------openToolStripMenuItem_Click
        /// <summary>
        /// Open a saved spreadsheet file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Create the next prompt window
                OpenPrompt prompt = new OpenPrompt(IPAddress);

                // open the Open window
                ThreadPool.QueueUserWorkItem(x => { prompt.ShowDialog(); });
            }
            catch (Exception)
            {
                MessageBox.Show("Server is not responding, please try again or close and reconnect", "ERROR");
            }

            //openFileDialog.Filter = "Spreadsheet Files (.ss)|*.ss|All Files (*.*)|*.*";
            //openFileDialog.Title = "Open Spreadsheet";
            //openFileDialog.InitialDirectory = @"C:\";
            //openFileDialog.RestoreDirectory = false;

            //DialogResult userClickedOK = openFileDialog.ShowDialog();
            //if (userClickedOK == DialogResult.OK)
            //{
            //    string filepath = openFileDialog.FileName;

            //    Form1 openForm = new Form1(model);

            //    SpreadsheetApplicationContext.getAppContext().RunForm(openForm);
            //    openForm.sheet = new Spreadsheet(filepath, s => true, s => s.ToUpper(), "ps6");

            //    IEnumerable<string> cellNames = openForm.sheet.GetNamesOfAllNonemptyCells();
            //    foreach (string name in cellNames)
            //    {
            //        int col, row;
            //        col = alphabet.IndexOf(name.Substring(0, 1));
            //        row = int.Parse(name.Substring(1)) - 1;

            //        openForm.spreadsheetPanel1.SetValue(col, row, openForm.sheet.GetCellValue(name).ToString());
            //        openForm.updateCell(openForm.spreadsheetPanel1, col, row);
            //    }
            //}
        }

        //---------------------------------------------saveToolStripMenuItem_Click
        /// <summary>
        /// Save a version of the spreadsheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();

            //saveFileDialog1.Filter = "Spreadsheet Files (.ss)|*.ss|All Files (*.*)|*.*";
            //saveFileDialog1.Title = "Save";
            //saveFileDialog1.InitialDirectory = @"C:\";
            //saveFileDialog1.RestoreDirectory = true;
            //saveFileDialog1.OverwritePrompt = true;

            //DialogResult userClickedOK = saveFileDialog1.ShowDialog();
            //if (userClickedOK == DialogResult.OK)
            //{
            //    string filepath = saveFileDialog1.FileName;
            //    sheet.Save(filepath);
            //}
        }

        //---------------------------------------------undoToolStripMenuItem_Click
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
        }

        //--------------------------------------------closeToolStripMenuItem_Click
        /// <summary>
        /// Closes the current Form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
            //if (sheet.Changed && (MessageBox.Show("Do you want to exit without saving?", "SpreadsheetProgram", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
            //{
            //    // Create the next prompt window
            //    //OpenPrompt prompt = new OpenPrompt(IPAddress);

            //    // Hide the window, leave this session, and open the Open window
            //    //this.Hide();
            //    //LeaveSession();
            //    //ThreadPool.QueueUserWorkItem(x => prompt.ShowDialog());
            //    //prompt.ShowDialog();

            //    Close();
            //}
            //if (!sheet.Changed)
            //{
            //    // Create the next prompt window
            //    //OpenPrompt prompt = new OpenPrompt(IPAddress);

            //    // Hide the window, leave this session, and open the Open window
            //    //this.Hide();
            //    //LeaveSession();
            //    //prompt.ShowDialog();

            //    Close();
            //}
        }

        //-----------------------------------------------------------Help_Click
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

        //----------------------------------------------------Form1_FormClosing
        /// <summary>
        /// Closes the spreadsheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (sheet.Changed && (MessageBox.Show("Do you want to exit without saving?", "SpreadsheetProgram", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
                {
                    
                    e.Cancel = false;
                    
                    // Create the next prompt window
                    //OpenPrompt prompt = new OpenPrompt(IPAddress);

                    // Hide the window, leave this session, and open the Open window
                    //this.Hide();

                    LeaveSession();
                    
                    //prompt.ShowDialog();
                    //this.Close();
                }
                else if (!sheet.Changed)
                {
                    e.Cancel = false;

                    // Create the next prompt window
                    //OpenPrompt prompt = new OpenPrompt(IPAddress);

                    // Hide the window, leave this session, and open the Open window
                    //this.Hide();

                    LeaveSession();

                    //prompt.ShowDialog();
                    //this.Close();
                }
                else
                    e.Cancel = true;
            }
            catch(Exception)
            {
            }
        }

        //------------------------------------------CellContentBox  start send here
        /// <summary>
        /// Allows the user to use the keyboard to control data entry from the
				/// keyboard.  While the CellContentBox is selected, the user must use
				/// the enter key to input the data into the spreadsheet, and may use the arrows
        /// to navigate the spreadsheet cells.  Also, when control + w is pressed,
				/// the option to close the Window will be presented to the user.
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
                    //SetCellContents(spreadsheetPanel1);
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
                    this.Close();

                if (e.Control && e.KeyCode == Keys.Z)
                    Undo();
                
                if (e.Control && e.KeyCode == Keys.O)
                {
                    try
                    {
                        // Create the next prompt window
                        OpenPrompt prompt = new OpenPrompt(IPAddress);

                        // open the Open window
                        ThreadPool.QueueUserWorkItem(x => { prompt.ShowDialog(); });
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Server is not responding, please try again or close and reconnect", "ERROR");
                    }
                }
                if (e.Control && e.KeyCode == Keys.S)
                    Save();
            }
            catch (Exception f)
            {
                MessageBox.Show("Idiot...\n" + f.ToString());
            }
        }

        //------------------------------------------------------updateSelection
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

        //---------------------------------------------------------SetCellContents
        /// <summary>
        /// A helper method to set the contents of the cell in the spreadsheetPanel.
				/// This will actually display on the panel the inputed information.
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

                foreach (string cell in affectedCells)
                {
                    int c, r;
                    c = alphabet.IndexOf(cell.Substring(0, 1));
                    r = int.Parse(cell.Substring(1)) - 1;

                    panel.SetValue(c, r, sheet.GetCellValue(cell).ToString());

                }
                string value = sheet.GetCellValue(name).ToString();

                panel.SetValue(col, row, value);

                updateSelection(panel);

            }
            catch (Exception f)
            {
                MessageBox.Show("Idiot.... (SetCellContents) \n" + f.Message);
            }
        }

        //-------------------------------------------------------------sendMessage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="panel"></param>
        private void sendMessage(SpreadsheetPanel panel)
        {
            Object prevContents = null;
            string name = "";

            try
            {
                int col, row;
                panel.GetSelection(out col, out row);
                name = alphabet[col] + (row + 1).ToString();

                prevContents = sheet.GetCellContents(name);
                string contents = CellContentBox.Text;

                ISet<string> affectedCells = sheet.SetContentsOfCell(name, contents);

                sheet.CircularCheck(name);
                Change(name, SSversion, contents);

            }
            catch (Exception f)
            {
                sheet.SetContentsOfCell(name, prevContents.ToString());
                MessageBox.Show("Idiot... (sendMessage) \n" + f.Message);
            }
        }

        //--------------------------------------------------------------updateCell
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

        //-----------------------------------------------------------updatePanel()
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

        //----------------------------------------------------------NavigateUpdate
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
        
        // ---------------------------------------------------------------New stuff

        // -----------------------------------------------------------------Sending
        /// <summary>
        /// This method will send a change to the server.
        /// </summary>
        /// <param name="cellName"></param>
        /// <param name="version"></param>
        /// <param name="content"></param>
        public void Change(String cellName, String version, String content)
        {
            // Send the message to the server, following protocol
            String message = "CHANGE \n";
            message += "Name:" + sheet.FileName + " \n";
            message += "Version:" + SSversion + " \n";
            message += "Cell:" + cellName + " \n";
            message += "Length:" + content.Length + " \n";
            message += content + " \n";

            // send change to server
            socket.BeginSend(message, (e, p) => { }, 0);
            //socket.BeginReceive(MessageReceived, "CHANGE");
        }

        /// <summary>
        /// This method will send an undo request to the server
        /// </summary>
        public void Undo()
        {
            // Create undo request, following protocol
            String message = "UNDO \n";
            message += "Name:" + sheet.FileName + " \n";
            message += "Version:" + SSversion + " \n";
            
            // Send message and receive more
            socket.BeginSend(message, (e, f) => { }, 0);
            //socket.BeginReceive(MessageReceived, 0);
        }

        /// <summary>
        /// This method will send a save request to the server
        /// </summary>
        public void Save()
        {
            // Create save request, following protocol
            String message = "SAVE \n";
            message += "Name:" + sheet.FileName + " \n";

            // send change to server
            socket.BeginSend(message, (e, p) => { }, 0);
            //socket.BeginReceive(MessageReceived, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public void LeaveSession()
        {
            // Create leave message, following protocol
            String message = "LEAVE \n";
            message += "Name:" + sheet.FileName + " \n";

            this.Hide();

            // send change to server
            socket.BeginSend(message, (e, p) => { }, 0);
        }

        //-----------------------------------------------------------Receiving
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void MessageReceived(String message, Exception e, Object p)
        {
            //MessageBox.Show(message);
            
            //if (messagesToReceive == 0)
            //{
                // Call the appropriate callback methods 
                if (message.Contains("CHANGE OK"))
                {
                    //SetCellContents(spreadsheetPanel1);

                    //messagesToReceive = 2;
                    socket.BeginReceive(ChangeReceived, "CHANGE");
                }
                else if (message.Contains("UNDO OK") || message.Contains("UPDATE"))
                {
                    //messagesToReceive = 5;
                    socket.BeginReceive(UpdateReceived, "UPDATE");
                }
                // If the save was successful, show a message
                else if (message.Contains("SAVE OK"))
                {
                    MessageBox.Show("Save successful", "Congratulations");

                    socket.BeginReceive(BlankReceived, "MESSAGE");
                }
                // If the undo reached an end, inform the user
                else if (message.Contains("UNDO END"))
                {
                    MessageBox.Show("No unsaved changes to undo.", "Undo");

                    //messagesToReceive = 2;
                    socket.BeginReceive(BlankReceived, "MESSAGE");
                }
                // If we received a wait, inform the user
                else if (message.Contains("WAIT"))
                {
                    // Create a string to report back to the user
                    String report = "Waiting for current version:\n";

                    // Append to the string with the appropriate message
                    if (message.Contains("CHANGE"))
                        report += "Change was not sent to server.\n";
                    else if (message.Contains("UNDO"))
                        report += "Undo was not sent to server.\n";

                    // Report the user of the wait
                    MessageBox.Show(report + "Wait for an update, or reconnect.", "Wait Error");

                    //messagesToReceive = 2;
                    socket.BeginReceive(BlankReceived, "MESSAGE");
                }
                else if (message.Contains("FAIL"))
                {
                    //messagesToReceive = 2;
                    socket.BeginReceive(FailReceived, "MESSAGE");
                }
                else
                {
                    socket.BeginReceive(MessageReceived, "MESSAGE");
                }
            //}
            //else
            //{
                // add to queue
                // call this method
                //socket.BeginReceive(BlankReceived, 0);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void ChangeReceived(String message, Exception e, Object p)
        {
            //MessageBox.Show(message);
            this.BeginInvoke(new Action(() => { SetCellContents(spreadsheetPanel1); }));
            if (message.Contains("Name:"))
            {
                sheet.FileName = message.Substring(5);
                //messagesToReceive--;
                socket.BeginReceive(ChangeReceived, 0);
                
            }
            else if (message.Contains("Version:"))
            {
                SSversion = message.Substring(8);

                //messagesToReceive--;
                socket.BeginReceive(MessageReceived, 0);
            }
            else
            {
                //messagesToReceive--;
                socket.BeginReceive(BlankReceived, "CHANGE");
            }
        }

        /// <summary>
        /// This callback method will deal with handling change messages
        ///  being sent from the server.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void VersionReceived(String message, Exception e, Object p)
        {
            if (message.Contains("Version:"))
            {
                SSversion = message.Substring(8);

                //messagesToReceive--;
                socket.BeginReceive(VersionReceived, "VERSION");
            }
            else
            {
                //messagesToReceive--;
                socket.BeginReceive(MessageReceived, "VERSION");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void UndoReceived(String message, Exception e, Object p)
        {
            if (message.Contains("Name:"))
            {
                sheet.FileName = message.Substring(5);
                socket.BeginReceive(UndoReceived, "UNDO");
            }
            else if (message.Contains("Version:"))
            {
                SSversion = message.Substring(8);
                socket.BeginReceive(UndoReceived, "UNDO");
            }
            // Cell name
            else if (message.Contains("Cell:"))
            {
                MessageBox.Show(message);
                modifyingCell = message.Substring(5);
                socket.BeginReceive(UndoReceived, "UNDO");
            }
            else if (message.Contains("Length:"))
            {
                socket.BeginReceive(UndoReceived, "UNDO");
            }
            else
            {
                this.Invoke(new Action(() => UndoMain(message)));

                 // change the content
                //ISet<string> affectedCells = new HashSet<String>();
                //this.Invoke(new Action(() => affectedCells = sheet.SetContentsOfCell(modifyingCell, message)));

                
                //foreach (string cell in affectedCells)
                //{
                //    int c, r;
                //    c = alphabet.IndexOf(cell.Substring(0, 1));
                //    r = int.Parse(cell.Substring(1)) - 1;

                //    spreadsheetPanel1.SetValue(c, r, sheet.GetCellValue(cell).ToString());

                //}
                //string value = sheet.GetCellValue(modifyingCell).ToString();

                //int cellNum = 0;
                //if (Int32.TryParse(modifyingCell.Substring(1, modifyingCell.Length - 1), out cellNum))
                //    spreadsheetPanel1.SetValue(alphabet.IndexOf(modifyingCell.Substring(0, 1)) + 1, cellNum, value);

                //updateSelection(spreadsheetPanel1);

                //socket.BeginReceive(MessageReceived, "UNDO");
            }

            ////Content
            //else if (!message.Contains("Name:") || !message.Contains("Length:"))
            //{
            //    // change the content
            //    ISet<string> affectedCells = sheet.SetContentsOfCell(modifyingCell, message);

            //    foreach (string cell in affectedCells)
            //    {
            //        int c, r;
            //        c = alphabet.IndexOf(cell.Substring(0, 1));
            //        r = int.Parse(cell.Substring(1)) - 1;

            //        spreadsheetPanel1.SetValue(c, r, sheet.GetCellValue(cell).ToString());

            //    }
            //    string value = sheet.GetCellValue(modifyingCell).ToString();

            //    int cellNum = 0;
            //    if (Int32.TryParse(modifyingCell.Substring(1, modifyingCell.Length - 1), out cellNum))
            //        spreadsheetPanel1.SetValue(alphabet.IndexOf(modifyingCell.Substring(0, 1)) + 1, cellNum, value);

            //    updateSelection(spreadsheetPanel1);

            //    // break out of the method so as to not receive more in this method
            //    messagesToReceive--;
            //    socket.BeginReceive(MessageReceived, 0);
            //    return;
            //}

            //messagesToReceive--;
            //socket.BeginReceive(MessageReceived, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public void UndoMain(String message)
        {
            // change the content
            ISet<string> affectedCells = new HashSet<String>();
            this.Invoke(new Action(() => affectedCells = sheet.SetContentsOfCell(modifyingCell, message)));


            foreach (string cell in affectedCells)
            {
                int c, r;
                c = alphabet.IndexOf(cell.Substring(0, 1));
                r = int.Parse(cell.Substring(1)) - 1;

                spreadsheetPanel1.SetValue(c, r, sheet.GetCellValue(cell).ToString());

            }
            string value = sheet.GetCellValue(modifyingCell).ToString();

            int cellNum = 0;
            if (Int32.TryParse(modifyingCell.Substring(1, modifyingCell.Length - 1), out cellNum))
                spreadsheetPanel1.SetValue(alphabet.IndexOf(modifyingCell.Substring(0, 1)) + 1, cellNum, value);

            updateSelection(spreadsheetPanel1);

            socket.BeginReceive(MessageReceived, "UNDO");
        }

        /// <summary>
        /// This callback method will receive an update from the server,
        ///  and change the specified cell on the spreadsheet with the
        ///  update.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void UpdateReceived(String message, Exception e, Object p)
        {
            // If the message contians the version, save it
            if (message.Contains("Version:"))
                SSversion = message.Substring(8);

            // If the message contains the name of the cell, save it
            else if (message.Contains("Cell:"))
            {
                modifyingCell = message.Substring(5);
            }
            // If the message contains the length of the receiving change, save it
            else if (message.Contains("Length:"))
                lengthCell = message.Substring(7);

            // If we've reached here, and the message doesn't contain name,
            //  we know it's the message that's been received
            else if (!message.Contains("Name:"))
            {
                // change the content
                ISet<string> affectedCells = sheet.SetContentsOfCell(modifyingCell, message);

                foreach (string cell in affectedCells)
                {
                    int c, r;
                    c = alphabet.IndexOf(cell.Substring(0, 1));
                    r = int.Parse(cell.Substring(1)) - 1;

                    spreadsheetPanel1.SetValue(c, r, sheet.GetCellValue(cell).ToString());

                }
                string value = sheet.GetCellValue(modifyingCell).ToString();

                int cellNum = 0;
                if (Int32.TryParse(modifyingCell.Substring(1, modifyingCell.Length - 1), out cellNum))
                    spreadsheetPanel1.SetValue(alphabet.IndexOf(modifyingCell.Substring(0, 1)), cellNum - 1, value);
                else
                    throw new InvalidNameException();
                this.Invoke(new Action(() => updateSelection(spreadsheetPanel1)));
                // Break out of the method so as not to receive more in this method
                //messagesToReceive--;
                socket.BeginReceive(MessageReceived, "UPDATE");
                return;
            }

            // Continue reading from the socket
            //messagesToReceive--;
            socket.BeginReceive(UpdateReceived, "UPDATE");
        }

        /// <summary>
        /// This is the callback method for when a "FAIL" is received.
        /// 
        /// It will display the message to the user containing the error of
        ///  why they received a fail.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void FailReceived(String message, Exception e, Object p)
        {
            // if the message doesn't contain name, it's the error
            if (!message.Contains("Name:"))
            {
                // display the error to the user
                MessageBox.Show(message, "Error");
                // messagesToReceive--;
                socket.BeginReceive(MessageReceived, "FAIL");
            }
            // If it does contain name, we need the next line being sent
            else
            {
                //messagesToReceive--;
                socket.BeginReceive(FailReceived, "FAIL");
            }
        }

        /// <summary>
        /// This callback method will handle dealing with save requests
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        public void BlankReceived(String message, Exception e, Object p)
        {
            if (p.Equals("CHANGE"))
            {
                socket.BeginReceive(ChangeReceived, "BLANK");
            }
            else if (p.Equals("UNDO"))
            {
                socket.BeginReceive(UndoReceived, "BLANK");
            }
            else if (p.Equals("UPDATE"))
            {
                socket.BeginReceive(UpdateReceived, "BLANK");
            }
            else if (p.Equals("MESSAGE"))
            {
                socket.BeginReceive(MessageReceived, "BLANK");
            }
            else if (p.Equals("FAIL"))
            {
                socket.BeginReceive(FailReceived, "BLANK");
            }
            else
                socket.BeginReceive(BlankReceived, "BLANK");
            //if (messagesToReceive == 0)
            //{
            //    //messagesToReceive--;
            //    socket.BeginReceive(MessageReceived, 0);
            //}
            //else
            //{
            //    //messagesToReceive--;
            //    socket.BeginReceive(BlankReceived, 0);
            //}
        }
    }
}
