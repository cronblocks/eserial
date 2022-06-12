using ESerial.SerialLib.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESerial.SerialLib
{
    public class SerialService
    {
        private PortFinder _portFinder;
        private List<string> _portNamesList;

        public SerialService()
        {
            _portFinder = new PortFinder();
            _portNamesList = new List<string>();
        }

        public void StartService()
        {
            _portNamesList.Clear();
        }

        public void StopService()
        {
            _portNamesList.Clear();
        }
    }
}
