using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace ESerial.SerialLib.Internals
{
    internal class PortsDiscoverer
    {
        private Timer _timer;
        private List<string> _foundPortsList;

        public PortsDiscoverer()
        {
            _foundPortsList = new List<string>();

            _timer = new Timer(FindPorts, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
        }

        private void FindPorts(object obj)
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                bool found = false;

                foreach (string existingPort in _foundPortsList)
                {
                    if (port == existingPort)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    _foundPortsList.Add(port);
                }
            }
        }

        internal void StartPortsDiscovery()
        {
            _timer.Change(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(50));
        }

        internal void StopPortsDiscovery()
        {
            _timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
            Thread.Sleep(200);
            _foundPortsList.Clear();
        }
    }
}
