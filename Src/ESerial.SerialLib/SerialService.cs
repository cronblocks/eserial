﻿using ESerial.SerialLib.Internals;
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
        public event Action PortDisconnected;
        public event Action<string> DataSent;
        public event Action<string> DataReceived;

        public string SerialPort { get; set; }
        public LineEnding LineEnding { get; set; } = LineEnding.None;
        public BaudRate BaudRate { get; set; } = BaudRate._115200;
        public uint InterLineTimeDelay { get; set; } = 1000; // Time delay in milliseconds

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

        #region Data Transmission Dispatchers
        private void OnPortDisconnected()
        {
            PortDisconnected?.Invoke();
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
