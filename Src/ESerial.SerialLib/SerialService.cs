using ESerial.SerialLib.Internals;
using ESerial.SerialLib.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESerial.SerialLib
{
    public class SerialService
    {
        public event Action<string> NewPortFound;
        public event Action<string> PortRemoved;

        public string SerialPort { get; set; }
        public LineEnding LineEnding { get; set; } = LineEnding.None;
        public BaudRate BaudRate { get; set; } = BaudRate._115200;
        public uint InterLineTimeDelay { get; set; } = 1000; // Time delay in milliseconds

        private PortsDiscoverer _portDiscoverer;
        private PortListener _portListener;

        #region Service Initialization & Control
        public SerialService()
        {
            _portDiscoverer = new PortsDiscoverer();
            _portDiscoverer.NewPortFound += OnNewPortFound;
            _portDiscoverer.PortRemoved += OnPortRemoved;
        }

        public void StartPortsDiscoveryService()
        {
            _portDiscoverer.StartPortsDiscovery();
        }

        public void StopPortsDiscoveryService()
        {
            _portDiscoverer.StopPortsDiscovery();
        }
        #endregion

        #region Port Discovery Dispatchers
        private void OnNewPortFound(string port)
        {
            NewPortFound?.Invoke(port);
        }

        private void OnPortRemoved(string port)
        {
            PortRemoved?.Invoke(port);
        }
        #endregion
    }
}
