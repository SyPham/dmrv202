using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRSelfHost.Helpers
{
    public class AppSettings
    {
        public string PortName { get; set; }
        public int MachineID { get; set; }
        public int CycleTime { get; set; }
        public string SignalrUrl { get; set; }
        public string LogPath { get; set; }
        public string[] Origins { get; set; }
    }
}
