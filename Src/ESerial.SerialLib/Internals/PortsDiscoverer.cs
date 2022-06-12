using System;
using System.Collections.Generic;
using System.IO.Ports;
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
            _timer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
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
            List<string> systemPorts = new List<string>(SerialPort.GetPortNames());
            List<string> portsToBeRemoved = new List<string>();

            // Finding newly added ports
            foreach (string port in systemPorts)
            {
                if (!_foundPortsList.Contains(port))
                {
                    _foundPortsList.Add(port);
                    NewPortFound?.Invoke(port);
                }
            }

            // Findling ports that no longer exist
            foreach (string port in _foundPortsList)
            {
                if (!systemPorts.Contains(port))
                {
                    portsToBeRemoved.Add(port);
                    PortRemoved?.Invoke(port);
                }
            }

            foreach (string port in portsToBeRemoved)
            {
                _foundPortsList.Remove(port);
            }
        }
        #endregion
    }
}
