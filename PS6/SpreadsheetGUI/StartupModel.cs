/*
 * This class will be used for a spreadsheet application client.
 * 
 * It will keep the connection active and keep the different spreadsheets
 * seperated by storing each one in a list.
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomNetworking;
using System.Net;
using System.Net.Sockets;

namespace SpreadsheetGUI
{
    /// <summary>
    /// 
    /// This is the main model of the Spreadsheet connection.
    /// 
    /// It will keep track of all of the SSModels that are open, where
    /// each SSModel represents a new spreadsheet that's open.  This will
    /// help to keep track of different spreadsheets open by one user.
    /// </summary>
    public class StartupModel
    {
        /// <summary>
        /// 
        /// </summary>
        public StringSocket socket { get; private set; }

        /* // maybe uneccessary stuff....
        /// <summary>
        /// This is an event listener for when the "New" button is pressed.
        /// </summary>
        public event Action<String> CreateEvent;
        
        /// <summary>
        /// Ths is an event listener for when the "Join" button is pressed.
        /// </summary>
        public event Action<String> JoinEvent;
        */

        private List<SSModel> modelList;
        
        /// <summary>
        /// Constructor for creating a new StartupModel.
        /// 
        /// It will intialize the neccessary member variables.
        /// </summary>
        public StartupModel()
        {
            // initialize the member variables
            modelList = new List<SSModel>();
            socket = null;
        }

        /// <summary>
        /// Used to connect the client to the server
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public SSModel Connect(String hostname, int port)
        {
            // If the socket has been initialized, create a connection
            if (socket == null)
            {
                // Try to connect to the server.
                TcpClient client = new TcpClient(hostname, port);
                socket = new StringSocket(client.Client, UTF8Encoding.UTF8);

                // Call the method to handle creating a new SSModelE
                return newSSModel();
            }
            // If the socket already contains something, connection is bad.
            else
                return null;
        }

        /// <summary>
        /// This method will be called to get a new SSModel to follow a spreadsheet.
        /// </summary>
        /// <returns></returns>
        public SSModel newSSModel()
        {
            // Create the model, add it to the list, and return it
            SSModel model = new SSModel(this);
            modelList.Add(model);
            return model;
        }
    }
}
