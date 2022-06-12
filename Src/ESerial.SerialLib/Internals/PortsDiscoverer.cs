using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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
            _timer.Elapsed += FindPorts;
        }

        private void FindPorts(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }

        internal void StartPortsDiscovery()
        {
            _timer.Start();
        }

        internal void StopPortsDiscovery()
        {
            _timer.Stop();
            Thread.Sleep(1000);
        }
    }
}
