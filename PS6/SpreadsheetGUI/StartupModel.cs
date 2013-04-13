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
    /// Stuff goes here
    /// </summary>
    public class StartupModel
    {
        StringSocket socket;


        /// <summary>
        /// This is an event listener for when the "New" button is pressed.
        /// </summary>
        public event Action<String> CreateEvent;
        
        /// <summary>
        /// Ths is an event listener for when the "Join" button is pressed.
        /// </summary>
        public event Action<String> JoinEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        public StartupModel()
        {
            socket = null;
        }

        /// <summary>
        /// Used to connect the client to the server
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(String hostname, int port)
        {
            if (socket == null)
            {
                // Try to connect to the server.
                TcpClient client = new TcpClient(hostname, port);
                socket = new StringSocket(client.Client, UTF8Encoding.UTF8);
                socket.BeginReceive(ConnectReceived, null);
            }
        }

        /// <summary>
        /// Callback method to call the other callback
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="payload"></param>
        public void ConnectReceived(String s, Exception e, object payload)
        {

        }
    }
}
