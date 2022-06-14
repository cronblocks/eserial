using ESerial.SerialLib.Types;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace ESerial.SerialLib.Internals
{
    internal class PortCommunicator
    {
        private const int WRITE_TIMEOUT_MILLISECONDS = 150;
        private const int READ_TIMEOUT_MILLISECONDS = 50;

        public event Action PortDisconnected;
        public event Action<string> DataSent;
        public event Action<string> DataReceived;

        private volatile bool _isRunning = false;
        private SerialPort _serialPort = null;
        private Thread _thread = null;

        public void StartPortTransactions(string portName, BaudRate baudRate)
        {
            if (!_isRunning)
            {
                _serialPort = new SerialPort(portName, (int)baudRate, Parity.None, 8, StopBits.One);
                _serialPort.ReadTimeout = READ_TIMEOUT_MILLISECONDS;
                _serialPort.WriteTimeout = WRITE_TIMEOUT_MILLISECONDS;
                _serialPort.Open();

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

                _thread.Join(500);
                _thread = null;
                
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort = null;
                }
            }
        }

        public void SendData(string data)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Write(data);
            }
            else
            {
                throw new Exception("Serial port is not working");
            }
        }

        private void PortDataReceiver(object _)
        {
            while (_isRunning)
            {
                try
                {
                    string receivedData = _serialPort.ReadLine();
                    if (!string.IsNullOrEmpty(receivedData))
                    {
                        DataReceived?.Invoke(receivedData);
                    }
                }
                catch (TimeoutException) { }
                catch (Exception)
                {
                    PortDisconnected?.Invoke();
                    try
                    {
                        _serialPort.Close();
                    }
                    catch (Exception) { }
                    _isRunning = false;
                }
            }
        }
    }
}
