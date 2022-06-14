using ESerial.SerialLib.Internals;
using ESerial.SerialLib.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ESerial.SerialLib
{
    public class SerialService
    {
        public event Action<string> NewPortFound;
        public event Action<string> PortRemoved;
        public event Action PortDisconnected;
        public event Action<string> DataSent;
        public event Action<string> DataReceived;

        public string SerialPort { get; set; }
        public LineEnding LineEnding { get; set; } = LineEnding.None;
        public BaudRate BaudRate { get; set; } = BaudRate._115200;
        public uint FileInterLineTimeDelay { get; set; } = 200; // Time delay in milliseconds

        private PortsDiscoverer _portDiscoverer;
        private PortCommunicator _portCommunicator;

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

        public void StartSerialPortTransactions()
        {
            if (!string.IsNullOrEmpty(SerialPort))
            {
                _portCommunicator = new PortCommunicator();
                _portCommunicator.PortDisconnected += OnPortDisconnected;
                _portCommunicator.DataSent += OnDataSent;
                _portCommunicator.DataReceived += OnDataReceived;
                _portCommunicator.StartPortTransactions(SerialPort, BaudRate);
            }
            else
            {
                throw new Exception("Invalid Serial Port");
            }
        }

        public void StopSerialPortTransactions()
        {
            if (_portCommunicator != null)
            {
                _portCommunicator.StopPortTransaction();
                _portCommunicator = null;
            }
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

        #region Data Transmissions & Dispatchers
        public void SendTextLineWithEndings(string line)
        {
            if (line != null && _portCommunicator != null)
            {
                _portCommunicator.SendData(line);

                switch (LineEnding)
                {
                    case LineEnding.None:
                        Debug.WriteLine($"Sending {line}");
                        break;

                    case LineEnding.CR:
                        Debug.WriteLine($"Sending {line}\\r");
                        _portCommunicator.SendData("\r");
                        break;

                    case LineEnding.LF:
                        Debug.WriteLine($"Sending {line}\\n");
                        _portCommunicator.SendData("\n");
                        break;

                    case LineEnding.CRLF:
                        Debug.WriteLine($"Sending {line}\\r\\n");
                        _portCommunicator.SendData("\r\n");
                        break;
                }
            }
        }

        public void TransmitFile(string filename)
        {

        }

        private void OnPortDisconnected()
        {
            PortDisconnected?.Invoke();
            _portCommunicator = null;
        }

        private void OnDataSent(string data)
        {
            DataSent?.Invoke(data);
        }

        private void OnDataReceived(string data)
        {
            DataReceived?.Invoke(data);
        }
        #endregion
    }
}
