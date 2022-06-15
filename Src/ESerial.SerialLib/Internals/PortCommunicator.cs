using ESerial.SerialLib.Types;
using System;
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

        public void StartPortTransactions(
            string portName,
            Types.BaudRate baudRate,
            Types.Parity parity,
            Types.StopBits stopBits)
        {
            if (!_isRunning)
            {
                _serialPort = new SerialPort(
                    portName,
                    (int)baudRate,
                    System.IO.Ports.Parity.None,
                    8,
                    System.IO.Ports.StopBits.One);
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
                DataSent?.Invoke(data);
            }
            else
            {
                throw new Exception("Serial port is not working");
            }
        }

        private void PortDataReceiver(object _)
        {
            byte[] readBytes = new byte[100];

            while (_isRunning)
            {
                try
                {
                    int totalReceived = _serialPort.Read(readBytes, 0, readBytes.Length);
                    if (totalReceived != 0)
                    {
                        DataReceived?.Invoke(Encoding.UTF8.GetString(readBytes, 0, totalReceived));
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
