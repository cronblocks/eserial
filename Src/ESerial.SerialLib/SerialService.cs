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

        private readonly string SETTINGS_DIRNAME;
        private const string SETTINGS_FILENAME = "Settings.ini";

        private PortsDiscoverer _portDiscoverer;
        private PortCommunicator _portCommunicator;
        private FileTransmitter _fileTransmitter;

        #region Service Initialization & Control
        public SerialService()
        {
            SETTINGS_DIRNAME = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ESerial";

            if (!Directory.Exists(SETTINGS_DIRNAME))
            {
                Directory.CreateDirectory(SETTINGS_DIRNAME);
            }

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
            try
            {
                using (StreamWriter file = new StreamWriter(SETTINGS_FILENAME))
                {
                    file.WriteLine($"LineEnding = {LineEnding}");
                    file.WriteLine($"BaudRate = {BaudRate.ToString().Replace("_", "")}");
                    file.WriteLine($"DataBits = {DataBits.ToString().Replace("_", "")}");
                    file.WriteLine($"Parity = {Parity}");
                    file.WriteLine($"StopBits = {StopBits}");
                    file.WriteLine($"FileInterLineTimeDelay = {FileInterLineTimeDelay}");
                }
            }
            catch (Exception) { }
        }

        private void LoadSettings()
        {
            if (File.Exists(SETTINGS_FILENAME))
            {
                try
                {
                    using (StreamReader file = new StreamReader(SETTINGS_FILENAME))
                    {
                        string line;
                        while ((line = file.ReadLine()) != null)
                        {
                            line = line.Trim();
                            
                            string[] parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                string name = parts[0].Trim();
                                string value = parts[1].Trim();

                                switch (name)
                                {
                                    case "LineEnding":
                                        if (Enum.TryParse(value, out LineEnding lineEnding))
                                        {
                                            LineEnding = lineEnding;
                                        }
                                        break;

                                    case "BaudRate":
                                        if (Enum.TryParse($"_{value}", out BaudRate baudRate))
                                        {
                                            BaudRate = baudRate;
                                        }
                                        break;

                                    case "DataBits":
                                        if (Enum.TryParse($"_{value}", out DataBits dataBits))
                                        {
                                            DataBits = dataBits;
                                        }
                                        break;

                                    case "Parity":
                                        if (Enum.TryParse(value, out Parity parity))
                                        {
                                            Parity = parity;
                                        }
                                        break;

                                    case "StopBits":
                                        if (Enum.TryParse(value, out StopBits stopBits))
                                        {
                                            StopBits = stopBits;
                                        }
                                        break;

                                    case "FileInterLineTimeDelay":
                                        if (uint.TryParse(value, out uint fileInterLineTimeDelay))
                                        {
                                            FileInterLineTimeDelay = fileInterLineTimeDelay;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        #endregion
    }
}
