using ESerial.SerialLib;
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
        }

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

        #region GUI Events Handling
        private void OnGuiLoaded(object sender, RoutedEventArgs e)
        {
            _serial.StartPortsDiscoveryService();
        }

        private void OnLineEndingChanged(object sender, RoutedEventArgs e)
        {
            if (LineEndingNone?.IsChecked == true)
            {
                Debug.WriteLine("Line Ending - None");
                _serial.LineEnding = SerialLib.Types.LineEnding.None;
            }
            else if (LineEndingCR?.IsChecked == true)
            {
                Debug.WriteLine("Line Ending - CR");
                _serial.LineEnding = SerialLib.Types.LineEnding.CR;
            }
            else if (LineEndingLF?.IsChecked == true)
            {
                Debug.WriteLine("Line Ending - LF");
                _serial.LineEnding = SerialLib.Types.LineEnding.LF;
            }
            else if (LineEndingCRLF?.IsChecked == true)
            {
                Debug.WriteLine("Line Ending - CRLF");
                _serial.LineEnding = SerialLib.Types.LineEnding.CRLF;
            }
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
