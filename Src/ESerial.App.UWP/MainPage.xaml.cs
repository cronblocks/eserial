using ESerial.SerialLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            this.InitializeComponent();

            _serial = new SerialService();
            _serial.NewPortFound += OnNewSerialPortFound;
            _serial.PortRemoved += OnSerialPortRemoved;
        }

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

        private void OnGuiLoaded(object sender, RoutedEventArgs e)
        {
            _serial.StartPortsDiscoveryService();
        }
    }
}
