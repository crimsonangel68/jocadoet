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

        public event Action<String> CreateEvent;
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
        /// <param name="callback"></param>
        public void Connect(String hostname, int port, Action callback)
        {
            if (socket == null)
            {
                // Try to connect to the server.
                TcpClient client = new TcpClient(hostname, port);
                socket = new StringSocket(client.Client, UTF8Encoding.Default);
                socket.BeginReceive(ConnectReceived, callback);
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
            OpenPrompt p = new OpenPrompt(null, 5, null);
            ((Action)payload)();
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="name"></param>
        public void Receiver(String name)
        {
            
        }
    }
}
