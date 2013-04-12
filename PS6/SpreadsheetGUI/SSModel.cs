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
    class SSModel
    {
        private StringSocket socket;

        public event Action<String> ChangeEvent;
        public event Action<String> UndoEvent;
        public event Action<String> SaveEvent;
        public event Action<String> LeaveEvent;

        //public SSModel(StringSocket thisSocket)
        public SSModel() // until socket is implemented !!!!!!!!!!!!!!!.........
        {
            //socket = thisSocket;
        }


    }
}
