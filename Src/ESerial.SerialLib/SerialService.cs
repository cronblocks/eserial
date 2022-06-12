﻿using ESerial.SerialLib.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESerial.SerialLib
{
    public class SerialService
    {
        public event Action<string> NewPortFound;
        public event Action<string> PortRemoved;

        private PortsDiscoverer _portDiscoverer;
        
        public SerialService()
        {
            _portDiscoverer = new PortsDiscoverer();
            _portDiscoverer.NewPortFound += OnNewPortFound;
            _portDiscoverer.PortRemoved += OnPortRemoved;
        }

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

        public void StartService()
        {
            _portDiscoverer.StartPortsDiscovery();
        }

        public void StopService()
        {
            _portDiscoverer.StopPortsDiscovery();
        }
    }
}
