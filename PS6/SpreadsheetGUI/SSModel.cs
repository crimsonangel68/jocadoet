using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomNetworking;
using System.Net;
using System.Net.Sockets;
using SpreadsheetGUI;

namespace SpreadsheetGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class SSModel
    {
        /// <summary>
        /// This property contains the parent model that the client is using to
        ///  connect to the server.
        ///  
        /// Can only be set within this class.
        /// </summary>
        public StartupModel startupModel { get; private set;}
        
        /// <summary>
        /// This property contains the name of the spreadsheet
        ///  associated with this SSModel.
        /// </summary>
        public string name { get; set; }

        /*
        // Unimplemented actions 
        public event Action<String> ChangeEvent;
        public event Action<String> UndoEvent;
        public event Action<String> SaveEvent;
        public event Action<String> LeaveEvent;
         */

        /// <summary>
        /// This constructor takes in a connected socket to
        ///  take care of sending and receiving.
        /// </summary>
        /// <param name="thisModel"></param>
        public SSModel(StartupModel thisModel) 
        {
          startupModel = thisModel;
        }
    }
}
