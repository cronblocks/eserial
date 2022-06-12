using ESerial.SerialLib.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESerial.SerialLib
{
    public class SerialService
    {
        private PortsDiscoverer _portDiscoverer;
        
        public SerialService()
        {
            _portDiscoverer = new PortsDiscoverer();
        }

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
