/*****************************************************************************************
 * Purpose:
 *     Providing a single interface to dependant application
 *
 *
 *****************************************************************************************
 * Author: Usama
 *
 *****************************************************************************************
 * Changes:
 *
 * Date              Description
 * ----              -----------
 *
 *
 *
 *
 *****************************************************************************************/

using ESerial.SerialLib.Internals;
using ESerial.SerialLib.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public event Action FileTransmissionStarted;
        public event Action FileTransmissionFinished;
        public event Action<int> FileTransmissionPercentageUpdated;

        public string SerialPort { get; set; }
        public LineEnding LineEnding { get; set; } = LineEnding.None;
        public BaudRate BaudRate { get; set; } = BaudRate._115200;
        public DataBits DataBits { get; set; } = DataBits._8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;
        public uint FileInterLineTimeDelay { get; set; } = 200; // Time delay in milliseconds

        private ISettingsProvider _settingsProvider;
        private PortsDiscoverer _portDiscoverer;
        private PortCommunicator _portCommunicator;
        private FileTransmitter _fileTransmitter;

        #region Service Initialization & Control
        public SerialService(ISettingsProvider settings)
        {
            _settingsProvider = settings;

            _portDiscoverer = new PortsDiscoverer();
            _portDiscoverer.NewPortFound += OnNewPortFound;
            _portDiscoverer.PortRemoved += OnPortRemoved;

            LoadSettings();
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
                _portCommunicator.StartPortTransactions(SerialPort, BaudRate, DataBits, Parity, StopBits);
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
            _fileTransmitter = new FileTransmitter(filename, this);
            _fileTransmitter.FileTransmissionStarted += OnFileTransmissionStarted;
            _fileTransmitter.FileTransmissionFinished += OnFileTransmissionFinished;
            _fileTransmitter.FileTransmissionPercentageUpdated += OnFileTransmissionPercentageUpdated;
            _fileTransmitter.StartTransmission();
        }

        private void OnPortDisconnected()
        {
            PortDisconnected?.Invoke();

            if (_fileTransmitter != null)
            {
                _fileTransmitter.QuitTransmission();
            }

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

        private void OnFileTransmissionStarted()
        {
            FileTransmissionStarted?.Invoke();
        }

        private void OnFileTransmissionFinished()
        {
            FileTransmissionFinished?.Invoke();
        }

        private void OnFileTransmissionPercentageUpdated(int percentage)
        {
            FileTransmissionPercentageUpdated?.Invoke(percentage);
        }
        #endregion

        #region Load & Save Settings
        public void StoreSettings()
        {
            _settingsProvider.SetString("LineEnding", LineEnding.ToString());
            _settingsProvider.SetString("BaudRate", BaudRate.ToString().Replace("_", ""));
            _settingsProvider.SetString("DataBits", DataBits.ToString().Replace("_", ""));
            _settingsProvider.SetString("Parity", Parity.ToString());
            _settingsProvider.SetString("StopBits", StopBits.ToString());
            _settingsProvider.SetString("FileInterLineTimeDelay", FileInterLineTimeDelay.ToString());

            _settingsProvider.SaveSettings();
        }

        private void LoadSettings()
        {
            if (Enum.TryParse(_settingsProvider.GetString("LineEnding"), out LineEnding lineEnding))
            {
                LineEnding = lineEnding;
            }

            if (Enum.TryParse($"_{_settingsProvider.GetString("BaudRate")}", out BaudRate baudRate))
            {
                BaudRate = baudRate;
            }

            if (Enum.TryParse($"_{_settingsProvider.GetString("DataBits")}", out DataBits dataBits))
            {
                DataBits = dataBits;
            }

            if (Enum.TryParse(_settingsProvider.GetString("Parity"), out Parity parity))
            {
                Parity = parity;
            }

            if (Enum.TryParse(_settingsProvider.GetString("StopBits"), out StopBits stopBits))
            {
                StopBits = stopBits;
            }

            if (uint.TryParse(_settingsProvider.GetString("FileInterLineTimeDelay"), out uint fileInterLineTimeDelay))
            {
                FileInterLineTimeDelay = fileInterLineTimeDelay;
            }
        }
        #endregion
    }
}
