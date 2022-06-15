using ESerial.SerialLib;
using ESerial.SerialLib.Types;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ESerial.App.WPF
{
    internal enum GuiState
    {
        TransmissionStopped,
        TransmissionStartedNormal,
        TransmissionStartedFile,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialService _serial;
        private FileStream? _dumpFileStream;

        private string _startButtonStartTitle = "";
        private string _startButtonStopTitle = "Stop";
        private string _dumpDirectoryName = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}ESerial-Dump{Path.DirectorySeparatorChar}";
        private string _dumpFilename = "";

        public MainWindow()
        {
            _serial = new SerialService();

            InitializeComponent();

            _startButtonStartTitle = (string)StartButton.Content;

            _serial.NewPortFound += OnNewSerialPortFound;
            _serial.PortRemoved += OnSerialPortRemoved;
            _serial.PortDisconnected += OnPortDisconnected;
            _serial.DataReceived += OnSerialPortDataReceived;
            _serial.DataSent += OnSerialPortDataSent;
            _serial.FileTransmissionStarted += OnFileTransmissionStarted;
            _serial.FileTransmissionFinished += OnFileTransmissionFinished;
            _serial.FileTransmissionPercentageUpdated += OnFileTransmissionPercentageUpdated;

            SetUiLineEndingOption();
            SetUiInterLineTimeDelay();
            SetUiBaudRates();
            SetUiParity();
            SetUiStopBits();
        }

        #region GUI Initialization
        private void SetUiLineEndingOption()
        {
            int currentIndex = 0;
            foreach (LineEnding lineEnding in Enum.GetValues(typeof(LineEnding)))
            {
                LineEndingComboBox.Items.Add(lineEnding.ToString());

                if (lineEnding == _serial.LineEnding)
                {
                    LineEndingComboBox.SelectedIndex = currentIndex;
                }

                currentIndex++;
            }
        }

        private void SetUiInterLineTimeDelay()
        {
            FileInterLineTimeDelayValueTextBox.Text = $"{_serial.FileInterLineTimeDelay}";
        }

        private void SetUiBaudRates()
        {
            int currentIndex = 0;
            foreach (BaudRate baudRate in Enum.GetValues(typeof(BaudRate)))
            {
                BaudRateComboBox.Items.Add(baudRate.ToString().Replace("_", ""));

                if (baudRate == _serial.BaudRate)
                {
                    BaudRateComboBox.SelectedIndex = currentIndex;
                }

                currentIndex++;
            }
        }

        private void SetUiParity()
        {
            int currentIndex = 0;
            foreach (Parity parity in Enum.GetValues(typeof(Parity)))
            {
                ParityComboBox.Items.Add(parity.ToString());

                if (parity == _serial.Parity)
                {
                    ParityComboBox.SelectedIndex = currentIndex;
                }

                currentIndex++;
            }
        }

        private void SetUiStopBits()
        {
            int currentIndex = 0;
            foreach (StopBits stopBits in Enum.GetValues(typeof(StopBits)))
            {
                switch (stopBits)
                {
                    case StopBits.One:
                        StopBitsComboBox.Items.Add("1");
                        break;

                    case StopBits.Two:
                        StopBitsComboBox.Items.Add("2");
                        break;

                    case StopBits.OnePointFive:
                        StopBitsComboBox.Items.Add("1.5");
                        break;
                }

                if (stopBits == _serial.StopBits)
                {
                    StopBitsComboBox.SelectedIndex = currentIndex;
                }

                currentIndex++;
            }
        }
        #endregion

        #region GUI State During App Execution
        private void SetGuiState(GuiState guiState)
        {
            switch (guiState)
            {
                case GuiState.TransmissionStopped:
                    MainAppWindow.Dispatcher.Invoke(
                        () =>
                        {
                            SerialPortHeading.IsEnabled = true;
                            SerialPortComboBox.IsEnabled = true;

                            BaudRateHeading.IsEnabled = true;
                            BaudRateComboBox.IsEnabled = true;

                            ParityHeading.IsEnabled = true;
                            ParityComboBox.IsEnabled = true;

                            StopBitsHeading.IsEnabled = true;
                            StopBitsComboBox.IsEnabled = true;

                            StartButton.Content = _startButtonStartTitle;

                            TransmitTextBox.IsEnabled = false;
                            TransmitButton.IsEnabled = false;
                            TransmitFileButton.IsEnabled = false;
                            TransmitFileProgressBar.Visibility = Visibility.Hidden;
                        });
                    break;

                case GuiState.TransmissionStartedNormal:
                    MainAppWindow.Dispatcher.Invoke(
                        () =>
                        {
                            SerialPortHeading.IsEnabled = false;
                            SerialPortComboBox.IsEnabled = false;

                            BaudRateHeading.IsEnabled = false;
                            BaudRateComboBox.IsEnabled = false;

                            ParityHeading.IsEnabled = false;
                            ParityComboBox.IsEnabled = false;

                            StopBitsHeading.IsEnabled = false;
                            StopBitsComboBox.IsEnabled = false;

                            StartButton.Content = _startButtonStopTitle;

                            TransmitTextBox.IsEnabled = true;
                            TransmitButton.IsEnabled = true;
                            TransmitFileButton.IsEnabled = true;
                            TransmitFileProgressBar.Visibility = Visibility.Hidden;
                        });
                    break;

                case GuiState.TransmissionStartedFile:
                    MainAppWindow.Dispatcher.Invoke(
                        () =>
                        {
                            SerialPortHeading.IsEnabled = false;
                            SerialPortComboBox.IsEnabled = false;

                            BaudRateHeading.IsEnabled = false;
                            BaudRateComboBox.IsEnabled = false;

                            ParityHeading.IsEnabled = false;
                            ParityComboBox.IsEnabled = false;

                            StopBitsHeading.IsEnabled = false;
                            StopBitsComboBox.IsEnabled = false;

                            StartButton.Content = _startButtonStopTitle;

                            TransmitTextBox.IsEnabled = false;
                            TransmitButton.IsEnabled = false;
                            TransmitFileButton.IsEnabled = false;
                            TransmitFileProgressBar.Visibility = Visibility.Visible;
                        });
                    break;
            }
        }
        #endregion

        #region GUI Events Handling
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _serial.StartPortsDiscoveryService();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _serial.StopPortsDiscoveryService();
            _serial.StopSerialPortTransactions();
            _serial.StoreSettings();

            try
            {
                _dumpFileStream?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"File Closing Error: {ex.Message}");
            }

            Environment.Exit(0);
        }

        private void OnSerialPortSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SerialPortComboBox.SelectedIndex >= 0)
            {
                _serial.SerialPort = SerialPortComboBox.Items[SerialPortComboBox.SelectedIndex].ToString();
            }
            else
            {
                _serial.SerialPort = "";
            }

            Debug.WriteLine($"Serial Port - {_serial.SerialPort}");
        }

        private void OnBaudRateSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BaudRateComboBox.SelectedIndex >= 0)
            {
                string enumStr = $"_{BaudRateComboBox.Items[BaudRateComboBox.SelectedIndex]}";

                if (Enum.IsDefined(typeof(BaudRate), enumStr))
                {
                    Enum.TryParse(typeof(BaudRate), enumStr, out object? baudRate);
                    if (baudRate != null)
                    {
                        _serial.BaudRate = (BaudRate)baudRate;
                    }
                }
            }

            Debug.WriteLine($"Baud Rate - {_serial.BaudRate.ToString().Replace("_", "")}");
        }

        private void OnDataBitsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataBitsComboBox.SelectedIndex >= 0)
            {
                string enumStr = $"_{DataBitsComboBox.Items[DataBitsComboBox.SelectedIndex]}";

                if (Enum.IsDefined(typeof(DataBits), enumStr))
                {
                    Enum.TryParse(typeof(DataBits), enumStr, out object? dataBits);
                    if (dataBits != null)
                    {
                        _serial.DataBits = (DataBits)dataBits;
                    }
                }
            }

            Debug.WriteLine($"Baud Rate - {_serial.BaudRate.ToString().Replace("_", "")}");
        }

        private void OnParitySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParityComboBox.SelectedIndex >= 0)
            {
                string enumStr = (string)ParityComboBox.Items[ParityComboBox.SelectedIndex];

                if (Enum.IsDefined(typeof(Parity), enumStr))
                {
                    Enum.TryParse(typeof(Parity), enumStr, out object? parity);
                    if (parity != null)
                    {
                        _serial.Parity = (Parity)parity;
                    }
                }
            }

            Debug.WriteLine($"Parity - {_serial.Parity}");
        }

        private void OnStopBitsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StopBitsComboBox.SelectedIndex >= 0)
            {
                string enumStr = (string)StopBitsComboBox.Items[StopBitsComboBox.SelectedIndex];

                switch (enumStr)
                {
                    case "1":
                        enumStr = StopBits.One.ToString();
                        break;

                    case "2":
                        enumStr = StopBits.Two.ToString();
                        break;

                    case "1.5":
                        enumStr = StopBits.OnePointFive.ToString();
                        break;
                }

                if (Enum.IsDefined(typeof(StopBits), enumStr))
                {
                    Enum.TryParse(typeof(StopBits), enumStr, out object? stopBits);
                    if (stopBits != null)
                    {
                        _serial.StopBits = (StopBits)stopBits;
                    }
                }
            }

            Debug.WriteLine($"Stop Bits - {_serial.StopBits}");
        }

        private void OnStartButtonClicked(object sender, RoutedEventArgs e)
        {
            if ((string)StartButton.Content == _startButtonStartTitle)
            {
                try
                {
                    if (!Directory.Exists(_dumpDirectoryName))
                    {
                        Directory.CreateDirectory(_dumpDirectoryName);
                    }

                    _dumpFilename = _dumpDirectoryName + $"ESerial-{DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")}.dump";
                }
                catch (Exception)
                {
                    _dumpFilename = $"ESerial-{DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")}.dump";
                }

                SaveToFileName.Text = $"Saving to: {_dumpFilename}";
                _dumpFileStream = new FileStream(_dumpFilename, FileMode.OpenOrCreate);

                try
                {
                    _serial.StartSerialPortTransactions();
                    SetGuiState(GuiState.TransmissionStartedNormal);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
            else
            {
                try
                {
                    _serial.StopSerialPortTransactions();
                    SetGuiState(GuiState.TransmissionStopped);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }

                try
                {
                    _dumpFileStream?.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"File Closing Error: {ex.Message}");
                }

                SaveToFileName.Text = $"Saved to: {_dumpFilename}";
            }
        }

        private void OnFileInterLineTimeDelayChanged(object sender, TextChangedEventArgs e)
        {
            if (uint.TryParse(FileInterLineTimeDelayValueTextBox.Text, out uint val))
            {
                _serial.FileInterLineTimeDelay = val;
                Debug.WriteLine($"Inter-line Time Delay - {val}ms");
            }
        }

        private void OnLineEndingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LineEndingComboBox.SelectedIndex >= 0)
            {
                string enumStr = (string)LineEndingComboBox.Items[LineEndingComboBox.SelectedIndex];

                if (Enum.IsDefined(typeof(LineEnding), enumStr))
                {
                    Enum.TryParse(typeof(LineEnding), enumStr, out object? lineEnding);
                    if (lineEnding != null)
                    {
                        _serial.LineEnding = (LineEnding)lineEnding;
                    }
                }
            }

            Debug.WriteLine($"Line Ending - {_serial.LineEnding}");
        }

        private void OnTransmitTextBoxKeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                OnTransmitButtonClicked(sender, new RoutedEventArgs());
            }
        }

        private void OnTransmitButtonClicked(object sender, RoutedEventArgs e)
        {
            _serial.SendTextLineWithEndings(TransmitTextBox.Text);
            TransmitTextBox.Text = "";
        }

        private void OnTransmitFileButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open text file for transmission";
            openFileDialog.FileName = "";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text documents (.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                Debug.WriteLine($"Transmitting file: {openFileDialog.FileName}");
                _serial.TransmitFile(openFileDialog.FileName);
            }
        }

        private void OnClearDisplayButtonClicked(object sender, RoutedEventArgs e)
        {
            MainTextBox.Dispatcher.Invoke(
                () =>
                {
                    MainTextBox.Text = "";
                    MainTextBox.ScrollToEnd();
                });
        }
        #endregion

        #region Serial Port Events Handling
        private void OnNewSerialPortFound(string port)
        {
            Debug.WriteLine($"Port Added: {port}");

            SerialPortComboBox.Dispatcher.Invoke(
                () =>
                {
                    SerialPortComboBox.Items.Add(port);
                    if (SerialPortComboBox.Items.Count == 1)
                    {
                        SerialPortComboBox.SelectedIndex = 0;
                    }
                });
        }

        private void OnSerialPortRemoved(string port)
        {
            Debug.WriteLine($"Port Removed: {port}");

            SerialPortComboBox.Dispatcher.Invoke(
                () =>
                {
                    SerialPortComboBox.Items.Remove(port);
                    if (SerialPortComboBox.Items.Count == 0)
                    {
                        SerialPortComboBox.SelectedIndex = -1;
                    }
                });
        }

        private void OnPortDisconnected()
        {
            Debug.WriteLine("Port Disconnected");
            SetGuiState(GuiState.TransmissionStopped);
        }

        private void OnSerialPortDataReceived(string data)
        {
            try
            {
                MainTextBox?.Dispatcher.Invoke(
                    () =>
                    {
                        MainTextBox.Text = MainTextBox.Text + data;
                        MainTextBox.ScrollToEnd();
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GUI Update Error: {ex.Message}");
            }

            try
            {
                byte[] dataBytes = Encoding.ASCII.GetBytes(data);
                _dumpFileStream?.Write(dataBytes, 0, dataBytes.Length);
                _dumpFileStream?.Flush();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"File Writing Error: {ex.Message}");
            }
        }

        private void OnSerialPortDataSent(string data)
        {
        }

        private void OnFileTransmissionStarted()
        {
            Debug.WriteLine($"File Transmission Started");
            SetGuiState(GuiState.TransmissionStartedFile);
        }

        private void OnFileTransmissionFinished()
        {
            Debug.WriteLine($"File Transmission Finished");
            
            MainAppWindow.Dispatcher.Invoke(() => { TransmitFileProgressBar.Value = 0; });
            
            SetGuiState(GuiState.TransmissionStartedNormal);
        }

        private void OnFileTransmissionPercentageUpdated(int percentage)
        {
            Debug.WriteLine($"Percentage Completed: {percentage}");

            MainAppWindow.Dispatcher.Invoke(() => { TransmitFileProgressBar.Value = percentage; });
        }
        #endregion
    }
}
