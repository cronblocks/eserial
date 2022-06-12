using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace ESerial.SerialLib.Internals
{
    internal class PortsDiscoverer
    {
        private Timer _timer;
        private List<string> _foundPortsList;

        public PortsDiscoverer()
        {
            _foundPortsList = new List<string>();
            _timer = new Timer(500);
        }

        internal void StartPortsDiscovery()
        {
            throw new NotImplementedException();
        }

        internal void StopPortsDiscovery()
        {
            throw new NotImplementedException();
        }
    }
}
