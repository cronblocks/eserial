using ESerial.SerialLib.Types;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace ESerial.SerialLib.Internals
{
    internal class PortListener
    {
        public event Action<string> DataReceived;
        public event Action<string> DataSent;

        private bool _isRunning = false;
        private SerialPort _serialPort = null;
        private Thread _thread = null;

        public void StartPortTransaction(string portName, BaudRate baudRate)
        {
            if (!_isRunning)
            {
                _serialPort = new SerialPort(portName, (int)baudRate, Parity.None, 8, StopBits.One);

                _isRunning = true;
                
                _thread = new Thread(PortDataReceiver);
                _thread.Start();
            }
        }

        public void StopPortTransaction()
        {
            if (_isRunning)
            {
                _isRunning = false;

                _thread.Join();
                _thread = null;
                
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort = null;
                }
            }
        }

        private void PortDataReceiver(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
