using ESerial.SerialLib;
using ESerial.SerialLib.Types;
using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ESerial.App.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SerialService _serial;

        public MainPage()
        {
            _serial = new SerialService();

            this.InitializeComponent();

            _serial.NewPortFound += OnNewSerialPortFound;
            _serial.PortRemoved += OnSerialPortRemoved;

            SetUiLineEndingOption();
            SetUiInterLineTimeDelay();
            SetUiBaudRates();
        }

        #region GUI Initialization
        private void SetUiLineEndingOption()
        {
            switch (_serial.LineEnding)
            {
                case SerialLib.Types.LineEnding.None:
                    LineEndingNone.IsChecked = true;
                    LineEndingCR.IsChecked = false;
                    LineEndingLF.IsChecked = false;
                    LineEndingCRLF.IsChecked = false;
                    break;

                case SerialLib.Types.LineEnding.CR:
                    LineEndingNone.IsChecked = false;
                    LineEndingCR.IsChecked = true;
                    LineEndingLF.IsChecked = false;
                    LineEndingCRLF.IsChecked = false;
                    break;

                case SerialLib.Types.LineEnding.LF:
                    LineEndingNone.IsChecked = false;
                    LineEndingCR.IsChecked = false;
                    LineEndingLF.IsChecked = true;
                    LineEndingCRLF.IsChecked = false;
                    break;

                case SerialLib.Types.LineEnding.CRLF:
                    LineEndingNone.IsChecked = false;
                    LineEndingCR.IsChecked = false;
                    LineEndingLF.IsChecked = false;
                    LineEndingCRLF.IsChecked = true;
                    break;
            }
        }

        private void SetUiInterLineTimeDelay()
        {
            InterLineTimeDelayValueTextBox.Text = $"{_serial.InterLineTimeDelay}";
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
        #endregion

        #region GUI Events Handling
        private void OnGuiLoaded(object sender, RoutedEventArgs e)
        {
            _serial.StartPortsDiscoveryService();
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
                string enumStr = $"_{BaudRateComboBox.Items[BaudRateComboBox.SelectedIndex].ToString()}";

                if (Enum.IsDefined(typeof(BaudRate), enumStr))
                {
                    Enum.TryParse(typeof(BaudRate), enumStr, out object baudRate);
                    _serial.BaudRate = (BaudRate)baudRate;
                }
            }

            Debug.WriteLine($"Baud Rate - {_serial.BaudRate.ToString().Replace("_", "")}");
        }

        private void OnInterLineTimeDelayChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (uint.TryParse(InterLineTimeDelayValueTextBox.Text, out uint val))
            {
                _serial.InterLineTimeDelay = val;
                Debug.WriteLine($"Inter-line Time Delay - {val}ms");
            }
        }

        private void OnLineEndingChanged(object sender, RoutedEventArgs e)
        {
            if (LineEndingNone?.IsChecked == true)
            {
                _serial.LineEnding = LineEnding.None;
            }
            else if (LineEndingCR?.IsChecked == true)
            {
                _serial.LineEnding = LineEnding.CR;
            }
            else if (LineEndingLF?.IsChecked == true)
            {
                _serial.LineEnding = LineEnding.LF;
            }
            else if (LineEndingCRLF?.IsChecked == true)
            {
                _serial.LineEnding = LineEnding.CRLF;
            }

            Debug.WriteLine($"Line Ending - {_serial.LineEnding}");
        }
        #endregion

        #region Serial Port Events Handling
        private void OnNewSerialPortFound(string port)
        {
            Debug.WriteLine($"Port Added: {port}");

            _ = SerialPortComboBox.Dispatcher.TryRunAsync(
                    CoreDispatcherPriority.Normal,
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

            _ = SerialPortComboBox.Dispatcher.TryRunAsync(
                    CoreDispatcherPriority.Normal,
                    () =>
                    {
                        SerialPortComboBox.Items.Remove(port);
                        if (SerialPortComboBox.Items.Count == 0)
                        {
                            SerialPortComboBox.SelectedIndex = -1;
                        }
                    });
        }
        #endregion
    }
}
