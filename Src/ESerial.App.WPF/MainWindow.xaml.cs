﻿using ESerial.SerialLib;
using ESerial.SerialLib.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ESerial.App.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialService _serial;

        public MainWindow()
        {
            _serial = new SerialService();

            InitializeComponent();

            _serial.NewPortFound += OnNewSerialPortFound;
            _serial.PortRemoved += OnSerialPortRemoved;
            _serial.DataReceived += OnSerialPortDataReceived;
            _serial.DataSent += OnSerialPortDataSent;

            SetUiLineEndingOption();
            SetUiInterLineTimeDelay();
            SetUiBaudRates();
        }

        #region GUI Initialization
        private void SetUiLineEndingOption()
        {
            switch (_serial.LineEnding)
            {
                case LineEnding.None:
                    LineEndingNone.IsChecked = true;
                    LineEndingCR.IsChecked = false;
                    LineEndingLF.IsChecked = false;
                    LineEndingCRLF.IsChecked = false;
                    break;

                case LineEnding.CR:
                    LineEndingNone.IsChecked = false;
                    LineEndingCR.IsChecked = true;
                    LineEndingLF.IsChecked = false;
                    LineEndingCRLF.IsChecked = false;
                    break;

                case LineEnding.LF:
                    LineEndingNone.IsChecked = false;
                    LineEndingCR.IsChecked = false;
                    LineEndingLF.IsChecked = true;
                    LineEndingCRLF.IsChecked = false;
                    break;

                case LineEnding.CRLF:
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
                    Enum.TryParse(typeof(BaudRate), enumStr, out object? baudRate);
                    if (baudRate != null)
                    {
                        _serial.BaudRate = (BaudRate)baudRate;
                    }
                }
            }

            Debug.WriteLine($"Baud Rate - {_serial.BaudRate.ToString().Replace("_", "")}");
        }

        private void OnInterLineTimeDelayChanging(TextBox sender, EventArgs args)
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

        private void OnClearButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void OnStartButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                _serial.StartSerialPortTransactions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");

            }
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

        private void OnSerialPortDataReceived(string data)
        {
            MainTextBox.Dispatcher.Invoke(
                () =>
                {
                    MainTextBox.Text = MainTextBox.Text + data;
                    MainTextBox.ScrollToEnd();
                });
        }

        private void OnSerialPortDataSent(string data)
        {
        }
        #endregion
    }
}
