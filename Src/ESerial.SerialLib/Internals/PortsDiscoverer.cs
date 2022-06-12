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

        public event Action<string> NewPortFound;
        public event Action<string> PortRemoved;

        public PortsDiscoverer()
        {
            _foundPortsList = new List<string>();

            _timer = new Timer(FindPorts, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
        }

        #region Starting and Stopping the Ports' Discovery
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
        #endregion

        #region Ports Discovery
        private void FindPorts(object obj)
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                if (!_foundPortsList.Contains(port))
                {
                    _foundPortsList.Add(port);
                    NewPortFound?.Invoke(port);
                }
            }
        }
        #endregion
    }
}
