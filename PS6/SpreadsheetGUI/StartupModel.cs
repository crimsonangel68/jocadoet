using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomNetworking;

namespace SpreadsheetGUI
{
    class StartupModel
    {
        StringSocket socket;

        public event Action<String> CreateEvent;
        public event Action<String> JoinEvent;

        public StartupModel()
        {
        }
    }
}
